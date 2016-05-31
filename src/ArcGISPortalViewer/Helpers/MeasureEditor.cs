// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see https://opensource.org/licenses/ms-pl for details.
// All other rights reserved

using System;
using System.Globalization;
using Windows.UI;
using ArcGISPortalViewer.Controls;
using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Symbology;

namespace ArcGISPortalViewer.Helpers
{
    /// <summary>
    /// Sub-class of the <see cref="Editor"/> used in <see cref="MeasureDisplayControl"/>
    /// </summary>
	[Windows.UI.Xaml.Data.Bindable]
    public class MeasureEditor : Editor
    {
        /// <summary>
        /// Used to override the symbols for midvertex with a different <see cref="SimpleMarkerSymbol"/>,
        /// vertex and selected vertex with a <see cref="CompositeSymbol"/> that contains <see cref="TextSymbol"/> 
        /// where <see cref="TextSymbol.Text"/> includes the coordinate index of vertex.
        /// </summary>
        /// <param name="generateSymbolInfo"><see cref="Editor.GenerateSymbolInfo"/></param>
        /// <returns>Generated Symbol</returns>
        protected override Symbol OnGenerateSymbol(GenerateSymbolInfo generateSymbolInfo)
        {
            if (generateSymbolInfo.GenerateSymbolType != GenerateSymbolType.Vertex &&
                generateSymbolInfo.GenerateSymbolType != GenerateSymbolType.SelectedVertex &&
                generateSymbolInfo.GenerateSymbolType != GenerateSymbolType.MidVertex)
                return base.OnGenerateSymbol(generateSymbolInfo);
            if (generateSymbolInfo.GenerateSymbolType == GenerateSymbolType.MidVertex)
                return new SimpleMarkerSymbol()
                {
                    Color = Color.FromArgb(1, 0, 0, 0),
                    Outline = new SimpleLineSymbol() {Width = 2, Color = Colors.White},
                    Size = 6
                };
            return new CompositeSymbol()
            {
                Symbols = new SymbolCollection(new Symbol[]
                {
                    new SimpleMarkerSymbol()
                    {
                        Color = Colors.White, Size = 14,
                        Outline = new SimpleLineSymbol() {Width = 1.5, Color = Colors.CornflowerBlue},
                    },
                    new TextSymbol()
                    {
                        Text =
                            Convert.ToString(generateSymbolInfo.VertexPosition.CoordinateIndex + 1, CultureInfo.InvariantCulture),
                        HorizontalTextAlignment = HorizontalTextAlignment.Center,
                        VerticalTextAlignment = VerticalTextAlignment.Middle
                    },
                })
            };
        }
    }
}