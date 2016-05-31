// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see https://opensource.org/licenses/ms-pl for details.
// All other rights reserved

using ArcGISPortalViewer.Popup.Converters;
using ArcGISPortalViewer.Popup.Primitives.Charting;
using Esri.ArcGISRuntime.WebMap;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

namespace ArcGISPortalViewer.Popup.Controls
{
    public sealed class Popup : ContentControl
    {
        #region Private Members

        private readonly Style _pageSubHeaderTextStyle = (Style)Application.Current.Resources["PageSubheaderTextStyle"];
        private readonly Style _baselineTextStyle = (Style)Application.Current.Resources["BaselineTextStyle"];
        private readonly double _controlContentThemeFontSize =(double) Application.Current.Resources["ControlContentThemeFontSize"];
        private readonly FontFamily _contentControlThemeFontFamily =(FontFamily) Application.Current.Resources["ContentControlThemeFontFamily"];

        #endregion Private Members

        #region Constructor

        public Popup()
        {
            DefaultStyleKey = typeof(Popup);
        }

        #endregion Constructor

        #region Public Properties

        #region PopupInfo

        public PopupInfo PopupInfo
        {
            get { return (PopupInfo)GetValue(PopupInfoProperty); }
            set { SetValue(PopupInfoProperty, value); }
        }

        public static readonly DependencyProperty PopupInfoProperty =
            DependencyProperty.Register("PopupInfo", typeof(PopupInfo), typeof(Popup), new PropertyMetadata(null, OnPopupInfoPropertyChanged));

        private static void OnPopupInfoPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Popup) d).UpdateTemplate();
        }

        #endregion PopupInfo

        #region Attributes

        public object Attributes
        {
            get { return GetValue(AttributesProperty); }
            set { SetValue(AttributesProperty, value); }
        }

        public static readonly DependencyProperty AttributesProperty =
            DependencyProperty.Register("Attributes", typeof(object), typeof(Popup), new PropertyMetadata(null, OnAttributesPropertyChanged));

        private static void OnAttributesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Popup)d).UpdateTemplate();
        }

        #endregion Attributes        

        #region Fields

        public IEnumerable<FieldInfo> Fields
        {
            get { return (IEnumerable<FieldInfo>)GetValue(FieldsProperty); }
            set { SetValue(FieldsProperty, value); }
        }
        
        public static readonly DependencyProperty FieldsProperty =
            DependencyProperty.Register("Fields", typeof(IEnumerable<FieldInfo>), typeof(Popup), new PropertyMetadata(null, OnFieldsPropertyChanged));

        private static void OnFieldsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Popup)d).UpdateTemplate();
        }

        #endregion Fields

        #endregion Public Properties

        #region Attached Properties

        #region Inline Collection

        public static readonly DependencyProperty InlineCollectionProperty =
                DependencyProperty.RegisterAttached("InlineCollection", typeof(object), typeof(Popup), new PropertyMetadata(null, OnInlineCollectionPropertyChanged));

        private static void OnInlineCollectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textblock = d as TextBlock;
            var newValue = e.NewValue as IEnumerable<Inline>;

            if (textblock != null && newValue != null)
            {
                textblock.Inlines.Clear();
                foreach (var line in newValue)
                {
                    textblock.Inlines.Add(line);
                }
            }
        }

        #endregion Inlien Collection

        #endregion Attached Properties

        #region Private Methods

        private void UpdateTemplate()
        {
            var stackPanel = new StackPanel { Orientation = Orientation.Vertical };
            Content = stackPanel;                       

            var titleText = new TextBlock
            {
                Style = _pageSubHeaderTextStyle,
                TextWrapping = TextWrapping.Wrap,
            };
            stackPanel.Children.Add(titleText);
            var hasTitle = PopupInfo != null && PopupInfo.Title != null && PopupInfo.Title.Trim().Length > 0;
            if (hasTitle)
            {
                titleText.SetBinding(InlineCollectionProperty, new Binding
                {
                    Path = new PropertyPath("Attributes"),
                    Source = this,
                    Converter = new StringFormatToInlineCollectionConverter(),
                    ConverterParameter = PopupInfo.Title
                });
            }
            if (PopupInfo != null && PopupInfo.Description != null && PopupInfo.Description.Trim().Length > 0)
            {
                if (!hasTitle)
                {
                    hasTitle = true;
                    titleText.SetBinding(InlineCollectionProperty, new Binding
                    {
                        Path = new PropertyPath("Attributes"),
                        Source = this,
                        Converter = new StringFormatToInlineCollectionConverter(),
                        ConverterParameter = PopupInfo.Description
                    });
                }
                var desc = new RichTextBlock
                {
                    FontSize = _controlContentThemeFontSize,
                    FontFamily = _contentControlThemeFontFamily
                };
                stackPanel.Children.Add(desc);

                var p = new Paragraph();
                desc.Blocks.Add(p);                
                
                BindingOperations.SetBinding(p,HtmlToTextConverter.HtmlToInlinesProperty, new Binding
                {
                    Path = new PropertyPath("Attributes"),
                    Source = this,
                    Converter = new HtmlToTextConverter(),
                    ConverterParameter = PopupInfo.Description
                });
            }
            else //Show attribute list
            {
                List<FieldInfo> displayFields = null;
                if (PopupInfo != null && PopupInfo.FieldInfos != null)
                {
                    displayFields = new List<FieldInfo>(PopupInfo.FieldInfos.Where(a => a.IsVisible));
                }
                if (displayFields == null)
                    return;
                var attributes = Attributes as IDictionary<string, object>;
                foreach (var item in displayFields)
                {
                    var sp = new StackPanel();
                    stackPanel.Children.Add(sp);
                    var l = new TextBlock
                    {
                        Style = _baselineTextStyle,
                        Margin = new Thickness(0, 10, 0, 0),
                        Text = item.Label ?? item.FieldName,
                        Foreground = new SolidColorBrush(Colors.DarkGray),
                        TextWrapping = TextWrapping.Wrap,
                        TextTrimming = TextTrimming.WordEllipsis                        
                    };
                    sp.Children.Add(l);
                    if (!hasTitle)
                    {
                        hasTitle = true;
                        titleText.SetBinding(InlineCollectionProperty, new Binding
                        {
                            Path = new PropertyPath(string.Format("Attributes[{0}]", item.FieldName)),
                            Source = this
                        });
                    }
                    var useHyperlink = attributes != null && attributes.ContainsKey(item.FieldName) &&
                        attributes[item.FieldName] is string && ((string)attributes[item.FieldName]).StartsWith("http");
                    if (useHyperlink || string.Equals("url", item.FieldName, StringComparison.OrdinalIgnoreCase))
                    {
                        var hyperlink = new HyperlinkButton();
                        sp.Children.Add(hyperlink);
                        hyperlink.SetBinding(HyperlinkButton.NavigateUriProperty,
                        new Binding
                        {
                            Path = new PropertyPath(string.Format("Attributes[{0}]", item.FieldName)),
                            Source = this
                        });
                        hyperlink.SetBinding(ContentProperty,
                        new Binding
                        {
                            Path = new PropertyPath(string.Format("Attributes[{0}]", item.FieldName)),
                            Source = this
                        });
                        hyperlink.Template = (ControlTemplate)XamlReader.Load(
                            "<ControlTemplate TargetType='HyperlinkButton' xmlns='http://schemas.microsoft.com/client/2007' >" +
                            "<TextBlock Text='{TemplateBinding Content}' Padding='0' Margin='0' RenderTransformOrigin='0.5,0.5' TextWrapping='Wrap' TextTrimming ='WordEllipsis'>" +
                            "<TextBlock.RenderTransform>" +
                            "<CompositeTransform TranslateY='5' />" +
                            "</TextBlock.RenderTransform>" +
                            "</TextBlock>" +
                            "</ControlTemplate>");
                        hyperlink.FontFamily = new FontFamily("Segoe UI Light");
                        hyperlink.FontWeight = FontWeights.Normal;
                        hyperlink.Margin = new Thickness(0);
                        hyperlink.Padding = new Thickness(0);
                    }
                    else
                    {
                        var t = new TextBlock
                        {
                            Style = _baselineTextStyle,
                            Margin = new Thickness(0, 10, 0, 0),
                            TextWrapping = TextWrapping.Wrap,
                            TextTrimming = TextTrimming.WordEllipsis
                        };
                        sp.Children.Add(t);
                        t.SetBinding(TextBlock.TextProperty, new Binding
                        {
                            Path = new PropertyPath(string.Format("Attributes[{0}]", item.FieldName)),
                            Source = this
                        });
                    }
                }
            }
            if (PopupInfo != null && PopupInfo.MediaInfos != null)
            {
                foreach (var item in PopupInfo.MediaInfos)
                {                   
                    if (!string.IsNullOrEmpty(item.Title))
                    {
                        var mediaTitle = new TextBlock
                        {
                            Style = _baselineTextStyle,
                            Margin = new Thickness(0, 10, 0, 0),
                            FontWeight = FontWeights.Bold,
                            TextWrapping = TextWrapping.Wrap,
                            TextTrimming = TextTrimming.WordEllipsis
                        };
                        stackPanel.Children.Add(mediaTitle);
                        if (!hasTitle)
                        {
                            hasTitle = true;
                            titleText.SetBinding(InlineCollectionProperty, new Binding
                            {
                                Path = new PropertyPath("Attributes"),
                                Source = this,
                                Converter = new StringFormatToInlineCollectionConverter(),
                                ConverterParameter = item.Title
                            });
                        }
                        mediaTitle.SetBinding(TextBlock.TextProperty, new Binding
                        {
                             Path = new PropertyPath("Attributes"),
                             Source = this,
                             Converter = new StringFormatToStringConverter(),
                             ConverterParameter = item.Title
                        });                        
                    }
                    if (!string.IsNullOrEmpty(item.Caption))
                    {
                        var mediaCaption = new TextBlock
                        {
                            Style = _baselineTextStyle,
                            Margin = new Thickness(0, 10, 0, 0),
                            FontStyle = FontStyle.Italic,
                            TextWrapping = TextWrapping.Wrap,
                            TextTrimming = TextTrimming.WordEllipsis
                        };
                        stackPanel.Children.Add(mediaCaption);
                        if (!hasTitle)
                        {
                            hasTitle = true;
                            titleText.SetBinding(InlineCollectionProperty, new Binding
                            {
                                Path = new PropertyPath("Attributes"),
                                Source = this,
                                Converter = new StringFormatToInlineCollectionConverter(),
                                ConverterParameter = item.Caption
                            });
                        }
                        mediaCaption.SetBinding(TextBlock.TextProperty, new Binding
                        {
                            Path = new PropertyPath("Attributes"),
                            Source = this,
                            Converter = new StringFormatToStringConverter(),
                            ConverterParameter = item.Caption
                        });                        
                    }

					IEnumerable<KeyValuePair<string,string>> fieldMappings = null;
					if (PopupInfo != null && PopupInfo.FieldInfos != null)
					{
						fieldMappings = from f in PopupInfo.FieldInfos
										select (new KeyValuePair<string, string>(f.FieldName, f.Label ?? f.FieldName));
					}
					BaseChart chart = null;
                    switch (item.Type)
                    {
                        case MediaType.Image:                        
                            var imageGrid = new Grid();
                            stackPanel.Children.Add(imageGrid);
                            if (!string.IsNullOrEmpty(item.Value.SourceUrl))
                            {
                                var image = new Image
                                {
                                    Margin = new Thickness(0,10,0,0),
                                    Width = 200d,
                                    Height = 200d,
                                    Stretch = Stretch.UniformToFill
                                };
                                imageGrid.Children.Add(image);
                                image.SetBinding(Image.SourceProperty, new Binding
                                {
                                    Path = new PropertyPath("Attributes"),
                                    Source = this,
                                    Converter = new StringFormatToBitmapSourceConverter(),
                                    ConverterParameter = item.Value.SourceUrl
                                });
                            }
                            if (!string.IsNullOrEmpty(item.Value.LinkUrl))
                            {
                                var hyperlinkButton = new HyperlinkButton
                                {
                                    Margin = new Thickness(0, 10, 0, 0),
                                    Width = 200d,
                                    Height = 200d
                                };
                                imageGrid.Children.Add(hyperlinkButton);
                                hyperlinkButton.SetBinding(HyperlinkButton.NavigateUriProperty, new Binding
                                {
                                    Path = new PropertyPath("Attributes"),
                                    Source = this,
                                    Converter = new StringFormatToUriConverter(),
                                    ConverterParameter = item.Value.LinkUrl
                                });
                            }
                            break;                        
                        case MediaType.BarChart:                            
								chart = new BarChart();
                                break;                            
                        case MediaType.ColumnChart:                           
                                chart = new ColumnChart();                             
                                break;                           
                        case MediaType.LineChart:                            
								chart = new LineChart();
                                break;                            
                        case MediaType.PieChart:                            
                                //string normalizeField = item.Value.NormalizeField;
                                chart = new PieChart();
                                break;                            
                    }
					if (chart != null)
					{
						var fieldString = string.Join(",", item.Value.Fields);
						var normalizeField = item.Value.NormalizeField;
						if (!string.IsNullOrEmpty(normalizeField) && !normalizeField.Equals("null"))
							fieldString += BaseChart.NormalizeSeparator + normalizeField;
                        chart.Margin = new Thickness(0, 10, 0, 0);
						chart.Fields = fieldString;
						chart.Height = 200;
						chart.Width = 200;
						chart.FontSize = 10d;
					    var keyValuePairs = fieldMappings as KeyValuePair<string, string>[] ?? fieldMappings.ToArray();
					    if (keyValuePairs.Any())
						{
							chart.KeyToLabelDictionary = new ResourceDictionary();
							foreach (var pair in keyValuePairs)
								chart.KeyToLabelDictionary[pair.Key] = pair.Value;
						}
						stackPanel.Children.Add(chart);
						chart.SetBinding(DataContextProperty, new Binding
						{
							Path = new PropertyPath("Attributes"),
							Source = this,
						});
					}
                }
            }        
        }

        #endregion Private Methods
        
    }
}