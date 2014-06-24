// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved

using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;

namespace ArcGISPortalViewer.Popup.Primitives.Charting
{
	/// <summary>
    /// *FOR INTERNAL USE ONLY* Column chart. 
	/// </summary>
    /// <exclude/>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class ColumnChart : BaseChart
	{
		private const double MinBarSize = 2;
		private const double MarginBetweenBars = 2;
		private const int MaxLabel = 7;

		private Grid _labels; // grid for labels
		private Grid _series; // grid for series of points

		/// <summary>
		/// Generates the column chart.
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
				GenerateChartStructure();

				// Generate the labels
				foreach (double val in labels)
					GenerateLabel(val, dataRange);

				// Generate data points
				foreach (var kvp in data)
				{
					GenerateDataPoint(kvp, dataRange);
				}
			}
		}

		private void GenerateChartStructure()
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

			// Axis 
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


		private void GenerateDataPoint(KeyValuePair<string, double> kvp, Range range)
		{
			// Add a column and add a grid in this column
			_series.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
			Grid point = new Grid();
			Grid.SetColumn(point, _series.ColumnDefinitions.Count - 1);
			_series.Children.Add(point);

			// Divide the grid in 3 rows
			double val = kvp.Value;
			double negativeFraction = range.Fraction(Math.Min(val, 0));
			double positiveFraction = 1 - range.Fraction(Math.Max(val, 0));

			point.RowDefinitions.Add(new RowDefinition { Height = new GridLength(positiveFraction, GridUnitType.Star) });
			point.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1 - positiveFraction - negativeFraction, GridUnitType.Star), MinHeight = MinBarSize});
			point.RowDefinitions.Add(new RowDefinition { Height = new GridLength(negativeFraction, GridUnitType.Star) });

			Border axisX = new Border
			{
				BorderThickness = new Thickness(0.75),
				BorderBrush = ForegroundColor,
				Opacity = 0.5,
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = val > 0 ? VerticalAlignment.Bottom : VerticalAlignment.Top,
				Margin = new Thickness(-5, 0, -5, 0)
			};
			Grid.SetRow(axisX, 1);
			point.Children.Add(axisX);

			// Put a bar in the middle row
			Border bar = new Border
			{
				Margin = new Thickness(MarginBetweenBars, 0, MarginBetweenBars, 0),
				Background = GetColorByIndex(0)
			};

			Grid.SetRow(bar, 1);
			SetTooltip(bar, kvp.Key, FormattedValue(kvp.Value));
			point.Children.Add(bar);
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

			Rectangle rect = new Rectangle { Stroke = ForegroundColor, Width = 5, Height = 1, Opacity = 0.5, Margin = new Thickness(2, 0, 0, 0) };
			stackPanel.Children.Add(rect);
		}

	}
}