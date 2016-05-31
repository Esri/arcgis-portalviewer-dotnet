// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see https://opensource.org/licenses/ms-pl for details.
// All other rights reserved

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcGISPortalViewer.Common
{
    /// <summary>
    /// Defines the format used for the coordinate notation string in the <see cref="MeasureDisplayControl"/>
    /// </summary>
    public enum CoordinateFormat
    {
        DecimalDegrees,
        Dms,
        DegreesDecimalMinutes,
        Mgrs
    }

    /// <summary>
    /// Defines the unit type used for the distance string in the <see cref="MeasureDisplayControl"/>
    /// </summary>
    public enum LinearUnitType
    {
        Metric,
        ImperialUS
    }
}
