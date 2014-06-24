// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236
using ArcGISPortalViewer.Common;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace ArcGISPortalViewer.Controls
{
    public sealed partial class SettingsControl : UserControl
    {
        public SettingsControl()
        {
            this.InitializeComponent();
        }
    }

    /// <summary>
    /// Converts <see cref="LinearUnitType"/> and <see cref="CoordinateFormat"/> setting enum to string.
    /// </summary>
    public sealed class SettingToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is LinearUnitType)
            {
                var unit = (LinearUnitType)value;
                switch (unit)
                {
                    case LinearUnitType.Metric:
                        return "Meters, Kilometers";
                    case LinearUnitType.ImperialUS:
                        return "Feet, Miles";
                }
            }
            else if (value is CoordinateFormat)
            {
                var format = (CoordinateFormat)value;
                switch (format)
                {
                    case CoordinateFormat.DecimalDegrees:
                        return "Decimal Degrees";
                    case CoordinateFormat.DegreesDecimalMinutes:
                        return "Degrees, Decimal Minutes";
                    case CoordinateFormat.Dms:
                        return "Degrees, Minutes, Seconds";
                    case CoordinateFormat.Mgrs:
                        return "Military Grid";
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}