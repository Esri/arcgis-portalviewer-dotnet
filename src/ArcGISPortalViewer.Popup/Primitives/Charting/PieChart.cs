// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved

using System;
using System.Linq;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace ArcGISPortalViewer.Popup.Primitives.Charting
{
    /// <summary>
    /// *FOR INTERNAL USE ONLY* Pie chart. 
	/// </summary>
    /// <exclude/>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public sealed class PieChart : BaseChart
    {
        /// <summary>
        /// Generates the pie chart.
        /// </summary>
        protected override void GenerateChart()
        {
            RootElement.Children.Clear();
            var data = ItemsSource;
            var validData = data == null ? null : data.Where(d => d.Value > 0);
            if (validData != null && validData.Any())
            {
                if (validData.Count() == 1)
                {
                    // Only one valid dat ==> draw a circle
                    int colorIndex = validData.Select((k, ind) => ind).FirstOrDefault();
                    var kvp = data.Where(d => d.Value > 0).First();

                    EllipseGeometry ellipseGeometry = new EllipseGeometry()
                    {
                        RadiusX = 1,
                        RadiusY = 1
                    };

                    Path path = new Path()
                    {
                        Stretch = Stretch.Uniform,
                        Fill = GetColorByIndex(colorIndex),
                        Data = ellipseGeometry
                    };
                    SetTooltip(path, kvp.Key, string.Format("{0} (100%)", FormattedValue(kvp.Value)));

                    RootElement.Children.Add(path);
                }
                else
                {
                    //We have more than one value to display. Generate pie shapes:
                    var total = validData.Sum(d => d.Value);
                    double current = 0;
                    int colorIndex = 0;
                    const double offset = 0; //Rotational offset (set it to Math.PI * .5 to start first segment upwards, set it to 0 to look like arcgis.com)

                    foreach (var kvp in data)
                    {
                        var val = kvp.Value;

                        if (val <= 0.0) // not valid
                        {
                            colorIndex++;
                            continue;
                        }

                        var fraction = val / total;

                        double angle0 = 2 * Math.PI * current - offset;
                        double angle1 = 2 * Math.PI * (current + fraction) - offset;

                        PathFigure pathFigure = new PathFigure { IsClosed = true, StartPoint = new Point(1, 1) };
                        pathFigure.Segments.Add(new LineSegment { Point = new Point(Math.Cos(angle0) + 1, Math.Sin(angle0) + 1) });
                        bool isLargeArc = fraction > .5;
                        pathFigure.Segments.Add(
                            new ArcSegment
                            {
                                Point = new Point(Math.Cos(angle1) + 1, Math.Sin(angle1) + 1),
                                IsLargeArc = isLargeArc,
                                Size = new Size(1, 1),
                                SweepDirection = SweepDirection.Clockwise
                            });
                        pathFigure.Segments.Add(new LineSegment() { Point = new Point(1, 1) });

                        //Add these two empty line segments to force the drawing to stretch properly 
                        //with 1,1 at the center ( 0,0->2,2 is the bounding box for the figure)
                        PathFigure pathFigure2 = new PathFigure() { StartPoint = new Point(0, 0) };
                        pathFigure2.Segments.Add(new LineSegment() { Point = new Point(0, 0) });
                        PathFigure pathFigure3 = new PathFigure() { StartPoint = new Point(2, 2) };
                        pathFigure3.Segments.Add(new LineSegment() { Point = new Point(2, 2) });

                        PathGeometry pathGeometry = new PathGeometry();
                        pathGeometry.Figures.Add(pathFigure2);
                        pathGeometry.Figures.Add(pathFigure3);
                        pathGeometry.Figures.Add(pathFigure);
                        Path path = new Path()
                        {
                            Stretch = Stretch.Uniform,
                            Fill = GetColorByIndex(colorIndex),
                            Data = pathGeometry
                        };

                        // Add outline separating pie slices
                        PathFigure outLineFigure = new PathFigure { IsClosed = false, StartPoint = new Point(1, 1) };
                        outLineFigure.Segments.Add(new LineSegment() { Point = new Point(Math.Cos(angle0) + 1, Math.Sin(angle0) + 1) });

                        //Add these two empty line segments to force the drawing to stretch properly 
                        //with 1,1 at the center ( 0,0->2,2 is the bounding box for the figure)
                        PathFigure outLineFigure2 = new PathFigure() { StartPoint = new Point(0, 0) };
                        outLineFigure2.Segments.Add(new LineSegment() { Point = new Point(0, 0) });
                        PathFigure outLineFigure3 = new PathFigure() { StartPoint = new Point(2, 2) };
                        outLineFigure3.Segments.Add(new LineSegment() { Point = new Point(2, 2) });

                        PathGeometry outLineGeometry = new PathGeometry();
                        outLineGeometry.Figures.Add(outLineFigure2);
                        outLineGeometry.Figures.Add(outLineFigure3);
                        outLineGeometry.Figures.Add(outLineFigure);
                        Path outLine = new Path()
                        {
                            Fill = null,
                            Stretch = Stretch.Uniform,
                            Stroke = new SolidColorBrush(Colors.White),
                            StrokeThickness = 1.0,
                            Data = outLineGeometry
                        };

                        SetTooltip(path, kvp.Key, string.Format("{0} ({1:0.##%})", FormattedValue(val), fraction));
                        RootElement.Children.Add(path);
                        RootElement.Children.Add(outLine);
                        current += fraction;
                        colorIndex++;
                    }
                }
            }
        }
    }
}