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
    /// *FOR INTERNAL USE ONLY* Bar chart. 
    /// </summary>
    /// <exclude/>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public sealed class BarChart : BaseChart
    {
        private const double MinBarSize = 2;
        private const double MarginBetweenBars = 2;
        private const int MaxLabel = 3;

        private Grid _labels; // grid for labels
        private Grid _series; // grid for series of points

        /// <summary>
        /// Generates the bar chart.
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
                var labels = GetLabelValues(dataRange, MaxLabel).ToArray();

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
            Grid root = new Grid { Margin = new Thickness(10, 0, 20, 0) }; // Horizontal Margin for labels
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Column for chart
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) }); // Column for labels
            RootElement.Children.Add(root);

            // Grid for labels (row 1)
            _labels = new Grid();
            Grid.SetRow(_labels, 1);
            root.Children.Add(_labels);

            // Grid for chart (row 0)
            Grid chart = new Grid();
            root.Children.Add(chart);

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
            chart.Children.Add(axisX);

            // Grid for series of points
            _series = new Grid();
            chart.Children.Add(_series);
        }


        private void GenerateDataPoint(KeyValuePair<string, double> kvp, Range range)
        {
            // Add a row and add a grid in this row
            _series.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            Grid point = new Grid();
            Grid.SetRow(point, _series.RowDefinitions.Count - 1);
            _series.Children.Add(point);

            // Divide the grid in 3 columns
            double val = kvp.Value;
            double negativeFraction = range.Fraction(Math.Min(val, 0));
            double positiveFraction = 1 - range.Fraction(Math.Max(val, 0));

            point.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(negativeFraction, GridUnitType.Star) });
            point.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1 - positiveFraction - negativeFraction, GridUnitType.Star), MinWidth = MinBarSize });
            point.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(positiveFraction, GridUnitType.Star) });

            Border axisY = new Border
            {
                BorderThickness = new Thickness(0.75),
                BorderBrush = ForegroundColor,
                Opacity = 0.5,
                HorizontalAlignment = val > 0.0 ? HorizontalAlignment.Left : HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Thickness(0, -5, 0, -5)
            };
            Grid.SetColumn(axisY, 1);
            point.Children.Add(axisY);

            // Put a bar in the middle column
            Border bar = new Border
            {
                Margin = new Thickness(0, MarginBetweenBars, 0, MarginBetweenBars),
                Background = GetColorByIndex(0)
            };

            Grid.SetColumn(bar, 1);
            SetTooltip(bar, kvp.Key, FormattedValue(kvp.Value));
            point.Children.Add(bar);
        }


        // Generate one label
        private void GenerateLabel(double val, Range dataRange)
        {
            double fraction = dataRange.Fraction(val);

            Grid label = new Grid();
            _labels.Children.Add(label);
            label.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(fraction, GridUnitType.Star) });
            label.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Pixel) });
            label.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1 - fraction, GridUnitType.Star) });

            StackPanel stackPanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Center,
                Orientation = Orientation.Vertical,
                Margin = new Thickness(-100, 0, -100, 0)
            };

            Grid.SetColumn(stackPanel, 1);
            label.Children.Add(stackPanel);

            Rectangle rect = new Rectangle { Stroke = ForegroundColor, Width = 1, Height = 5, Opacity = 0.5, Margin = new Thickness(2, 0, 0, 0) };
            stackPanel.Children.Add(rect);

            TextBlock text = new TextBlock
            {
                Text = FormattedValue(val)
            };
            stackPanel.Children.Add(text);
        }

        internal static IEnumerable<double> GetLabelValues(Range dataRange, double maxLabel)
        {
            double maxValue = Math.Max(-dataRange.Min, dataRange.Max);
            if (maxValue <= 0.0) // all values equal to 0
            {
                yield return 0.0;
            }
            else
            {
                double nbDigit = Math.Ceiling(Math.Log10(maxValue / maxLabel));
                double step = Math.Pow(10, nbDigit); // increment the label by a power of 10

                if (step * 1.5 > maxValue)
                    step /= 2; // label 5 by 5 else only one label that may be far and thus inelegant

                for (double current = 0.0; current <= dataRange.Max + step / 2.0; current += step)
                {
                    yield return current;
                }
                for (double current = -step; current >= dataRange.Min - step / 2.0; current -= step)
                {
                    yield return current;
                }
            }
        }

    }

    internal class Range
    {
        public Range(double min, double max)
        {
            Min = min;
            Max = max;
        }

        public double Min { get; set; }
        public double Max { get; set; }

        public double Fraction(double val)
        {
            return Max == Min ? 0 : (val - Min) / (Max - Min);
        }
    }
}