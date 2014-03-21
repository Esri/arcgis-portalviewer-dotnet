using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace ArcGISPortalViewer.Popup.Primitives.Charting
{
	/// <summary>
    /// *FOR INTERNAL USE ONLY* Line chart. 
	/// </summary>
    /// <exclude/>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class LineChart : BaseChart
	{
		private const int MaxLabel = 7;

		private Grid _labels; // grid for labels
		private Grid _series; // grid for series of points

		/// <summary>
		/// Generates the line chart.
		/// </summary>
		protected override void GenerateChart()
		{
			RootElement.Children.Clear();
			var data = ItemsSource;
			if (data != null && data.Any())
			{
				double max = Math.Max(0, data.Values.Max());
				double min = Math.Min(0, data.Values.Min());
				Range dataRange = new Range(min, max);

				// Get the label values
				var labels = BarChart.GetLabelValues(dataRange, MaxLabel).ToArray();

				// The label might be out of the current range (e.g if max value = '99', there will be a label '100')
				// Extend the range to include the labels
				dataRange.Min = Math.Min(dataRange.Min, labels.Min());
				dataRange.Max = Math.Max(dataRange.Max, labels.Max());

				// Create chart main structure
				GenerateChartStructure(dataRange);

				// Generate the labels
				foreach (double val in labels)
					GenerateLabel(val, dataRange);

				// Generate data points
				GenerateDataPoints(data, dataRange);
			}
		}

		private void GenerateChartStructure(Range range)
		{
			// Main grid
			Grid root = new Grid { Margin = new Thickness(0, 10, 0, 10) }; // Vertical Margin for labels
			root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Auto) }); // Column for labels
			root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Column for chart
			RootElement.Children.Add(root);

			// Grid for labels (column 0)
			_labels = new Grid();
			root.Children.Add(_labels);

			// Grid for chart (column 1)
			Grid chart = new Grid();
			Grid.SetColumn(chart, 1);
			root.Children.Add(chart);

			// Grid for Axis in coordinate 0
			Grid gridAxis = new Grid();
			double fraction = range.Fraction(0);

			gridAxis.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1 - fraction, GridUnitType.Star) });
			gridAxis.RowDefinitions.Add(new RowDefinition { Height = new GridLength(fraction, GridUnitType.Star) });
			chart.Children.Add(gridAxis);

			// Axis 
			Border axisX = new Border
			{
				BorderThickness = new Thickness(0.75),
				BorderBrush = ForegroundColor,
				Opacity = 0.5,
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Bottom,
				Margin = new Thickness(-5, 0, -5, 0)
			};
			gridAxis.Children.Add(axisX);

			Border axisY = new Border
			               	{
			               		BorderThickness = new Thickness(0.75),
			               		BorderBrush = ForegroundColor,
			               		Opacity = 0.5,
			               		HorizontalAlignment = HorizontalAlignment.Left,
			               		VerticalAlignment = VerticalAlignment.Stretch,
			               		Margin = new Thickness(0, -5, 0, -5)
							};
			chart.Children.Add(axisY);

			// Grid for series of points
			_series = new Grid();
			chart.Children.Add(_series);
		}


		private void GenerateDataPoints(IDictionary<string, double> data, Range dataRange)
		{
			int count = data.Count;

			// Project the data in order to get the points in percent of the drawing space (rectangle 1*1)
			var points = data.Select((kvp, ind) => new { x = (2.0 * ind + 1.0) / (2.0 * count), y = 1.0 - dataRange.Fraction(kvp.Value)});

			// Generate the lines from point to point
			var firstPoint = points.First();
			PathFigure pathFigure = new PathFigure { IsClosed = false, StartPoint = new Point(firstPoint.x, firstPoint.y) };
			foreach (var point in points.Skip(1))
			{
				pathFigure.Segments.Add(new LineSegment {Point = new Point(point.x, point.y)});
			}

			//Add these two empty line segments to force the drawing to stretch properly 
			PathFigure pathFigure2 = new PathFigure { StartPoint = new Point(0, 0) };
			pathFigure2.Segments.Add(new LineSegment { Point = new Point(0, 0) });
			PathFigure pathFigure3 = new PathFigure { StartPoint = new Point(1, 1) };
			pathFigure3.Segments.Add(new LineSegment { Point = new Point(1, 1) });

			PathGeometry pathGeometry = new PathGeometry();
			pathGeometry.Figures.Add(pathFigure2);
			pathGeometry.Figures.Add(pathFigure3);
			pathGeometry.Figures.Add(pathFigure);
			Path path = new Path
			{
				Stretch = Stretch.Fill, // stretch the polyline to fill the chart drawing space
				Stroke = GetColorByIndex(0),
				StrokeThickness = 2,
				StrokeLineJoin = PenLineJoin.Round,
				Data = pathGeometry,
				Margin = new Thickness(-1,-1,-1,-1) // half of strokeThickness
			};
			_series.Children.Add(path);

			// Add a new grid for the points
			Grid pointsGrid = new Grid();
			_series.Children.Add(pointsGrid);

			// Generate the points
			foreach (var kvp in data)
			{
				GenerateDataPoint(pointsGrid, kvp, dataRange);
			}
		}

		private void GenerateDataPoint(Grid points, KeyValuePair<string, double> kvp, Range range)
		{
			// Add a column and add a grid in this column
			points.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
			Grid point = new Grid();
			Grid.SetColumn(point, points.ColumnDefinitions.Count - 1);
			points.Children.Add(point);

			// Divide the grid in 3 rows
			double val = kvp.Value;
			double fraction = range.Fraction(val);

			point.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1 - fraction, GridUnitType.Star) });
			point.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Pixel) });
			point.RowDefinitions.Add(new RowDefinition { Height = new GridLength(fraction, GridUnitType.Star) });

			// Put a point in the middle row
			Ellipse ellipse = new Ellipse
			                  	{
			                  		Height = 10,
			                  		Width = 10,
			                  		Fill = GetColorByIndex(0),
			                  		HorizontalAlignment = HorizontalAlignment.Center,
			                  		VerticalAlignment = VerticalAlignment.Center,
			                  		Margin = new Thickness(0, -20, 0, -20),
			                  		Stroke = ForegroundColor,
			                  		StrokeThickness = 1
								};

			Grid.SetRow(ellipse, 1);
			SetTooltip(ellipse, kvp.Key, FormattedValue(kvp.Value));
			point.Children.Add(ellipse);
		}


		// Generate one label
		private void GenerateLabel(double val, Range dataRange)
		{
			double fraction = dataRange.Fraction(val);

			Grid label = new Grid();
			_labels.Children.Add(label);
			label.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1 - fraction, GridUnitType.Star) });
			label.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Pixel) });
			label.RowDefinitions.Add(new RowDefinition { Height = new GridLength(fraction, GridUnitType.Star) });

			StackPanel stackPanel = new StackPanel
			                        	{
			                        		VerticalAlignment = VerticalAlignment.Center,
			                        		HorizontalAlignment = HorizontalAlignment.Right,
			                        		Orientation = Orientation.Horizontal,
			                        		Margin = new Thickness(0, -20, 0, -20)
			                        	};

			Grid.SetRow(stackPanel, 1);
			label.Children.Add(stackPanel);

			TextBlock text = new TextBlock
			{
				Text = FormattedValue(val)
			};
			stackPanel.Children.Add(text);

			Rectangle rect = new Rectangle { Stroke = ForegroundColor, Width = 5, Height = 1, Opacity = 0.5, Margin = new Thickness(2,0,0,0) };
			stackPanel.Children.Add(rect);
		}

	}
}