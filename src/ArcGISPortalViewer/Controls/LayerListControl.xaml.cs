// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved

using Esri.ArcGISRuntime.Layers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace ArcGISPortalViewer.Controls
{
	public sealed partial class LayerListControl : UserControl
	{
		public LayerListControl()
		{
			this.InitializeComponent();
		}

		public IEnumerable<Layer> Layers
		{
			get { return (IEnumerable<Layer>)GetValue(LayersProperty); }
			set { SetValue(LayersProperty, value); }
		}

		public static readonly DependencyProperty LayersProperty =
			DependencyProperty.Register("Layers", typeof(object), typeof(LayerListControl), new PropertyMetadata(null, OnLayerListControlPropertyChanged));

		private static void OnLayerListControlPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var ctrl = d as LayerListControl;
			ctrl.LayerItems.ItemsSource = ctrl.Layers;
		}
	}

	public class ThumbTooltipValueConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value is double)
			{
				return string.Format("{0:0}%", ((double)value) * 100);
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
