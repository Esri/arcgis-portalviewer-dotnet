// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Windows.Foundation.Collections;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using ArcGISPortalViewer.Popup.Utilities;
using Windows.UI.Xaml.Controls;

namespace ArcGISPortalViewer.Popup.Converters
{
    public class HtmlToTextConverter : IValueConverter
    {
        private static string htmlLineBreakRegex = @"(<br */>)|(\[br */\])|(</?p.*?>)|(</?div.*?>)|(</?span.*?>)|(</?b>)|(</?i>)|(</?ul>)|(</?ol>)|(</?li>)|(</?a.*?>)|(<img.*?/>)";
        private static string htmlStripperRegex = @"<(.|\n)*?>";	// Regular expression to strip HTML tags        


        public static string GetHtmlToInlines(DependencyObject obj)
        {
            return (string)obj.GetValue(HtmlToInlinesProperty);
        }

        public static void SetHtmlToInlines(DependencyObject obj, string value)
        {
            obj.SetValue(HtmlToInlinesProperty, value);
        }

        // Using a DependencyProperty as the backing store for HtmlToInlinesProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HtmlToInlinesProperty =
            DependencyProperty.RegisterAttached("HtmlToInlines", typeof(string), typeof(HtmlToTextConverter), new PropertyMetadata(null, OnHtmlToInlinesPropertyChanged));

        private static void OnHtmlToInlinesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var p = d as Paragraph;
            if (p != null)
            {
                if (e.NewValue == null)
                    p.Inlines.Clear();
                else
                {
                    SetParagraphInlineCollection(p, CreateInlineCollection(e.NewValue as string));
                }
            }
            else
            {
                var r = d as RichTextBlock;
                if (r != null)
                {
                    r.Blocks.Clear();
                    var np = new Paragraph()
                    {
                        FontSize = (double)Application.Current.Resources["ControlContentThemeFontSize"],
                        FontFamily = Application.Current.Resources["ContentControlThemeFontFamily"] as FontFamily
                    };
                    SetParagraphInlineCollection(np, CreateInlineCollection(e.NewValue as string));
                    r.Blocks.Add(np);
                }
            }
        }

        private static void SetParagraphInlineCollection(Paragraph p, List<Inline> newValue)
        {
            p.Inlines.Clear();
            foreach (Inline line in newValue)
            {
                p.Inlines.Add(line);
            }
        }

        private static List<Inline> CreateInlineCollection(string newValue)
        {
            var inlines = new List<Inline>();
            bool bold = false;
            bool list_item = false;
            bool unordered_list = false;
            bool ordered_list = false;
            string href = "";
            int ordered_number = -1;
            int linebreak_count = 0;
            var splits = Regex.Split(newValue, htmlLineBreakRegex, RegexOptions.IgnoreCase | RegexOptions.ECMAScript);
            foreach (var line in splits)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var line_lowercase = line.ToLower();
                if (IsHtmlTag(line_lowercase))
                {
                    switch (line_lowercase)
                    {
                        case "</div>":
                        case "</p>":
                        case "<br/>":
                        case "<br />":
                        case "<br>":
                            if (!ordered_list && !unordered_list)
                                WriteLineBreak(inlines, ref linebreak_count);
                            break;
                        case "<bold>":
                        case "<b>":
                            bold = true;
                            break;
                        case "</bold>":
                        case "</b>":
                            bold = false;
                            break;
                        case "<ul>":
                            WriteLineBreak(inlines, ref linebreak_count);
                            unordered_list = true;
                            break;
                        case "</ul>":
                            unordered_list = false;
                            break;
                        case "<ol>":
                            WriteLineBreak(inlines, ref linebreak_count);
                            ordered_list = true;
                            ordered_number = 1;
                            break;
                        case "</ol>":
                            ordered_list = false;
                            ordered_number = -1;
                            break;
                        case "<li>":
                            list_item = true;
                            break;
                        case "</li>":
                            list_item = false;
                            WriteLineBreak(inlines, ref linebreak_count);
                            break;
                        case "</a>":
                            href = "";
                            break;
                    }
                }

                if (line_lowercase == "<p>" || (line_lowercase.StartsWith("<p ") && line_lowercase.EndsWith(">")))
                {
                    if (!ordered_list && !unordered_list)
                        WriteLineBreak(inlines, ref linebreak_count);
                }

                if (line_lowercase.StartsWith("<a ") && line_lowercase.Contains("href="))
                {
                    char quote = line_lowercase.Contains("href='") ? '\'' : '"';
                    int start_index = line_lowercase.IndexOf(string.Format("href={0}", quote)) + 6;
                    int end_index = line_lowercase.IndexOf(string.Format("{0}", quote), start_index);
                    href = line.Substring(start_index, end_index - start_index);
                }

                if (line_lowercase.StartsWith("<img") && line_lowercase.Contains("src='"))
                {
                    int start_index = line_lowercase.IndexOf("src='") + 5;
                    int end_index = line.IndexOf("'", start_index);
                    string src = line.Substring(start_index, end_index - start_index);

                    var image = new Image() { Source = new BitmapImage(new Uri(src, UriKind.Absolute)) };
                    image.Stretch = Stretch.None;

                    var inline_ui_container = new InlineUIContainer();
                    inline_ui_container.Child = image;

                    inlines.Add(inline_ui_container);
                    WriteLineBreak(inlines, ref linebreak_count);
                    WriteLineBreak(inlines, ref linebreak_count);
                }

                string text = Regex.Replace(line, htmlStripperRegex, string.Empty);
                Regex regex = new Regex(@"[ ]{2,}", RegexOptions.None);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    text = regex.Replace(text, @" "); //Remove multiple spaces
                    text = text.Replace("&quot;", "\""); //Unencode quotes
                    text = text.Replace("&nbsp;", " "); //Unencode spaces
                    var run = new Run() { Text = text };

                    if (bold)
                        run.FontWeight = FontWeights.SemiBold;

                    if (unordered_list && list_item)
                    {
                        run.Text = run.Text.Insert(0, "•  ");
                        list_item = false;
                    }

                    if (ordered_list && list_item)
                    {
                        run.Text = run.Text.Insert(0, string.Format("{0}.  ", ordered_number++));
                        list_item = false;
                    }

                    if (!string.IsNullOrEmpty(href))
                    {
                        int pos = 0;
                        foreach (var str in text.Split(new char[] { ' ', '/' }))
                        {
                            var word = str;
                            pos += word.Length;
                            if (pos < text.Length)
                            {
                                word += text[pos];
                                pos++;
                            }
                            var hyperlink = new HyperlinkButton();
                            hyperlink.NavigateUri = new Uri(href, UriKind.Absolute);
                            hyperlink.Content = word;
                            hyperlink.Template = (ControlTemplate)XamlReader.Load(
                                "<ControlTemplate TargetType='HyperlinkButton' xmlns='http://schemas.microsoft.com/client/2007' >" +
                                "<TextBlock Text='{TemplateBinding Content}' Padding='0' Margin='0' RenderTransformOrigin='0.5,0.5'>" +
                                "<TextBlock.RenderTransform>" +
                                "<CompositeTransform TranslateY='5' />" +
                                "</TextBlock.RenderTransform>" +
                                "</TextBlock>" +
                                "</ControlTemplate>");
                            hyperlink.FontFamily = new FontFamily("Segoe UI Light");
                            hyperlink.FontWeight = FontWeights.Normal;
                            hyperlink.Margin = new Thickness(0);
                            if (word.EndsWith(" "))
                                hyperlink.Margin = new Thickness(0, 0, 4, 0);
                            hyperlink.Padding = new Thickness(0);
                            var inline_ui_container = new InlineUIContainer();
                            inline_ui_container.Child = hyperlink;

                            inlines.Add(inline_ui_container);
                        }
                    }
                    else
                        inlines.Add(run);

                    linebreak_count = 0;
                }
            }
            return inlines;
        }

        private static void WriteLineBreak(List<Inline> inlines, ref int line_break_counter)
        {
            if (line_break_counter < 2)
            {
                inlines.Add(new LineBreak());
                line_break_counter++;
            }
        }

        public static string GetHtmlToWebView(DependencyObject obj)
        {
            return (string)obj.GetValue(HtmlToWebViewProperty);
        }

        public static void SetHtmlToWebView(DependencyObject obj, string value)
        {
            obj.SetValue(HtmlToWebViewProperty, value);
        }

        // Using a DependencyProperty as the backing store for HtmlToWebView.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HtmlToWebViewProperty =
            DependencyProperty.RegisterAttached("HtmlToWebView", typeof(string), typeof(HtmlToTextConverter), new PropertyMetadata(null, OnHtmlToWebViewPropertyChanged));

        private static void OnHtmlToWebViewPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var WebView = d as WebView;
            WebView.NavigateToString((string)e.NewValue);
        }

        private static bool IsHtmlTag(string str)
        {
            return str.StartsWith("<") && str.EndsWith(">");
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string)
                return ToStrippedHtmlText(value);
            else
            {
                var dict = value as IDictionary<string, object>;
                var formatter = parameter as string;
                if (dict == null || string.IsNullOrWhiteSpace(formatter))
                    return null;
                else if (!string.IsNullOrEmpty(formatter))
                {
                    value = AttributeBindingHelper.ResolveBinding(dict, formatter);
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        internal static string ToStrippedHtmlText(object input)
        {
            string retVal = string.Empty;

            if (input != null)
            {
                // Replace HTML line break tags with $LINEBREAK$:
                retVal = Regex.Replace(input as string, htmlLineBreakRegex, "$LINEBREAK$", RegexOptions.IgnoreCase);
                // Remove the rest of HTML tags:
                retVal = Regex.Replace(retVal, htmlStripperRegex, string.Empty);
#if !SILVERLIGHT
                // In WPF all { characters must appear after a \ in XAML:
                retVal = retVal.Replace("{", "\\{");
#endif
            }

            return retVal;
        }

    }
}
