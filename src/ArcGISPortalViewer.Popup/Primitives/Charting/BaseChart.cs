using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace ArcGISPortalViewer.Popup.Primitives.Charting
{
	/// <summary>
	/// FOR INTERNAL USE ONLY. Base class for charts. 
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public abstract class BaseChart : Control
	{
		#region Private Members

		private readonly string[] _chartColors = { // 15 colors
		                            "284B70", // Blue
		                            "702828", // Red
		                            "5F7143", // Light Green
		                            "F6BC0C", // Yellow
		                            "382C6C", // Indigo
		                            "50224F", // Magenta
		                            "1D7554", // Dark Green
		                            "4C4C4C", // Gray Shade
		                            "0271AE", // Light Blue
		                            "706E41", // Brown
		                            "446A73", // Cyan
		                            "0C3E69", // Medium Blue
		                            "757575", // Gray Shade 2
		                            "B7B7B7", // Gray Shade 3
		                            "A3A3A3" // Gray Shade 4
		                        };
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="BaseChart"/> class.
		/// </summary>
		internal BaseChart()
		{
			DefaultStyleKey = typeof(BaseChart);
		}
		
		#endregion

		#region Dependency Property Fields
		public const string NormalizeSeparator = "_::_"; // separator for the normalize field in Fields

		///<summary>
		/// Fields displayed into the chart (e.g Field1,Field2,Field3 or  Field1,Field2,Field3_::_NormalizeField)
		///</summary>
		public string Fields
		{
			get { return (string)GetValue(FieldsProperty); }
			set { SetValue(FieldsProperty, value); }
		}

		/// /// <summary>
		/// Identifies the <see cref="Fields"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty FieldsProperty =
			DependencyProperty.Register("Fields", typeof(string), typeof(BaseChart), new PropertyMetadata(null, OnFieldsPropertyChanged));

		static void OnFieldsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((BaseChart)d).SetItemsSource();
		}

		#endregion

		#region  Dependency Property KeyToLabelDictionary
		///<summary>
		/// Dictionary with the labels by field.
		///</summary>
		public ResourceDictionary KeyToLabelDictionary
		{
			get { return (ResourceDictionary)GetValue(KeyToLabelDictionaryProperty); }
			set { SetValue(KeyToLabelDictionaryProperty, value); }
		}

		/// /// <summary>
		/// Identifies the <see cref="KeyToLabelDictionary"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty KeyToLabelDictionaryProperty =
			DependencyProperty.Register("KeyToLabelDictionary", typeof(ResourceDictionary), typeof(BaseChart), new PropertyMetadata(null, OnKeyToLabelDictionaryPropertyChanged));

		static void OnKeyToLabelDictionaryPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((BaseChart)d).SetItemsSource();
		}
		#endregion

		#region Dependency Property ItemsSource
		///<summary>
		/// Dictionary with the values to display in the chart
		///</summary>
		public IDictionary<string, double> ItemsSource
		{
			get { return (IDictionary<string, double>)GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}

		/// <summary>
		/// Identifies the <see cref="ItemsSource"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ItemsSourceProperty =
			DependencyProperty.Register("ItemsSource", typeof(IDictionary<string, double>), typeof(BaseChart), new PropertyMetadata(null, OnItemsSourceChanged));

		private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((BaseChart)d).InvalidateChart();
		}
		#endregion

		/// <summary>
		/// When overridden in a derived class, is invoked whenever application
		/// code or internal processes (such as a rebuilding layout pass) call
		/// <see cref="M:System.Windows.Controls.Control.ApplyTemplate"/>. In
		/// simplest terms, this means the method is called just before a UI 
		/// element displays in an application.
		/// </summary>
		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			RootElement = GetTemplateChild("Root") as Grid;
			if (RootElement != null) 
				GenerateChart();
		}

		/// <summary>
		/// Overrides MeasureOverride
		/// </summary>
		/// <param name="availableSize"></param>
		/// <returns></returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			// Chart stretchs on available size
			return availableSize;
		}

		#region ForegroundColor
		///<summary>
		/// Color for chart axis and stroke of line chart points
		///</summary>
		protected Brush ForegroundColor
		{
			get
			{
				// Use the foreground color of the control 
				return GetValue(Control.ForegroundProperty) as Brush;
			}
		}

		#endregion

		#region Protected Methods

		///<summary>
		/// Set the tooltip of a data point
		///</summary>
		protected virtual void SetTooltip(DependencyObject element, string key, string value)
		{
			ToolTipService.SetToolTip(element, 
				string.Format("{0} : {1}", key, value));
		}

		/// <summary>
		/// Generates the chart from the ItemsSource
		/// </summary>
		protected abstract void GenerateChart();

		/// <summary>
		/// Root element of the control
		/// </summary>
		protected Grid RootElement { get; private set; }

		/// <summary>
		/// Invalidates the chart
		/// </summary>
		protected void InvalidateChart()
		{
			if (RootElement != null)
				GenerateChart();
		}

		/// <summary>
		/// Returns a color by index
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		protected Brush GetColorByIndex(int index)
		{
			string chartColor = _chartColors[index % _chartColors.Count()];
			Color color = Color.FromArgb(0xff, byte.Parse(chartColor.Substring(0, 2), NumberStyles.HexNumber), byte.Parse(chartColor.Substring(2, 2), NumberStyles.HexNumber), byte.Parse(chartColor.Substring(4, 2), NumberStyles.HexNumber));
			return new SolidColorBrush(color);
		}

		/// <summary>
		/// Returns a double value formatted to be displayed in axis or tooltip.
		/// </summary>
		/// <param name="value"></param>
		/// <returns>Label</returns>
		protected string FormattedValue(double value)
		{
			// Use group separator
			return value.ToString("#,0.##########");
		}
		#endregion

		#region Private Methods

		private void SetItemsSource()
		{
			if (!string.IsNullOrEmpty(Fields))
			{
				var binding = new Binding
				{
					Path = new PropertyPath(""),
					Converter = new ChartConverter { KeyToLabelDictionary = KeyToLabelDictionary },
					ConverterParameter = Fields
				};
				SetBinding(ItemsSourceProperty, binding);
			}
		} 
		#endregion

	}

	#region ChartConverter
	/// <summary>
	/// Converts and filters the attributes for use with displaying charting in popup.
	/// </summary>
	internal sealed class ChartConverter : IValueConverter
	{
		public ResourceDictionary KeyToLabelDictionary { get; set; }

        /// <summary>
        /// Modifies the source data before passing it to the target for display in the UI.
        /// </summary>
        /// <param name="value">The source data being passed to the target.</param>
        /// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the target dependency property.</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        /// <returns>
        /// The value to be passed to the target dependency property.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            IDictionary<string, double> pairs = new Dictionary<string, double>();
            if (parameter is String && value is IDictionary<string, object>)
            {
                var attributes = value as IDictionary<string, object>;
                var fields = (parameter as string);
                var normalizeFieldInd = fields.LastIndexOf(BaseChart.NormalizeSeparator);
                double normalizeValue = 0.0;
                if (normalizeFieldInd > 0)
                {
                    var normalizeField = fields.Substring(normalizeFieldInd + BaseChart.NormalizeSeparator.Length);
                    fields = fields.Substring(0, normalizeFieldInd);
                    if (attributes.ContainsKey(normalizeField))
                    {
                        try
                        {
                            normalizeValue = System.Convert.ToDouble(attributes[normalizeField], CultureInfo.InvariantCulture);
                        }
                        catch (Exception)
                        {
                            normalizeValue = 0.0;
                        }
                    }
                }
                foreach (var field in fields.Split(new[] { ',' }))
                {
                    if (attributes.ContainsKey(field))
                    {
                        string label = field;
                        if (KeyToLabelDictionary != null && KeyToLabelDictionary.ContainsKey(field))
                            label = KeyToLabelDictionary[field] as string;

                        double val;
                        try
                        {
                            val = System.Convert.ToDouble(attributes[field], CultureInfo.InvariantCulture);
                        }
                        catch (Exception)
                        {
                            val = 0.0;
                        }
                        if (normalizeValue != 0.0)
                            val /= normalizeValue;
                        pairs.Add(new KeyValuePair<string, double>(label, val));
                    }
                }
            }
            return pairs;
        }

        /// <summary>
        /// Modifies the target data before passing it to the source object.  This method is called only in <see cref="F:System.Windows.Data.BindingMode.TwoWay"/> bindings.
        /// </summary>
        /// <param name="value">The target data being passed to the source.</param>
        /// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the source object.</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        /// <returns>
        /// The value to be passed to the source object.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
	#endregion

}
