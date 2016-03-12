// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved

using System;
using System.Linq;
using System.Text;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace ArcGISPortalViewer.Controls
{
    /// <summary>
    /// Simplifies rendering generated HTML and handling link events without navigating the view.
    /// </summary>
    public sealed partial class HtmlPresenterControl : UserControl
    {
        #region Private Members

        private bool _allowDefaultNavigation;

        #endregion Private Members

        #region Constructor

        public HtmlPresenterControl()
        {
            this.InitializeComponent();
        }

        #endregion Constructor

        #region Public Properties

        #region Html

        public string Html
        {
            get { return (string)GetValue(HtmlProperty); }
            set { SetValue(HtmlProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Html.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HtmlProperty =
            DependencyProperty.Register("Html", typeof(string), typeof(HtmlPresenterControl), new PropertyMetadata(null, OnHtmlPropertyChanged));

        private static void OnHtmlPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HtmlPresenterControl)d).LoadContent();
        }

        #endregion Html

        #region HtmlFontFamily

        public string HtmlFontFamily
        {
            get { return (string)GetValue(HtmlFontFamilyProperty); }
            set { SetValue(HtmlFontFamilyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HtmlFontFamily.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HtmlFontFamilyProperty =
            DependencyProperty.Register("HtmlHtmlFontFamily", typeof(string), typeof(HtmlPresenterControl), new PropertyMetadata(null, OnFontFamilyPropertyChanged));

        private static void OnFontFamilyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HtmlPresenterControl)d).LoadContent();
        }

        #endregion HtmlFontFamily

        #region HtmlFontSize

        public string HtmlFontSize
        {
            get { return (string)GetValue(HtmlFontSizeProperty); }
            set { SetValue(HtmlFontSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HtmlFontSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HtmlFontSizeProperty =
            DependencyProperty.Register("HtmlFontSize", typeof(string), typeof(HtmlPresenterControl), new PropertyMetadata(null, OnHtmlFontSizePropertyChanged));

        private static void OnHtmlFontSizePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HtmlPresenterControl)d).LoadContent();
        }

        #endregion HtmlFontSize

        #endregion Public Properties

        #region Public Events

        public event EventHandler<NotifyEventArgs> OnLinkClicked;

        #endregion Public Events

        #region Private Methods

        private void LoadContent()
        {

            const string script = @"window.onclick = hrefEvent;
                                function hrefEvent(e)
                                {
	                                e = e || window.event;
	                                var t = e.target || e.srcElement

                                    while( t.nodeName != 'A' && t.parentNode != null)
                                        t = t.parentNode;
    
	                                if ( t.href )                                    
                                        window.external.notify(t.href);	                                                                                            

                                    return false;
                                }";

            var bodyAttributes = new StringBuilder();

            // Font-Family:
            if (!string.IsNullOrEmpty(HtmlFontFamily))
                bodyAttributes.AppendFormat(" font-family:'{0}'; ", HtmlFontFamily);

            // Font-Size;
            if (!string.IsNullOrEmpty(HtmlFontSize))
                bodyAttributes.AppendFormat(" font-size:{0}; ", HtmlFontSize);

            var content = string.Format("<!DOCTYPE html><html><style>h2{{font-family:'segoe ui light';font-weight:bold;font-size:15pt;}}</style><body style=\"{2}\"><script>{0}</script>{1}</body></html>", script, Html, bodyAttributes);

            _allowDefaultNavigation = true;
            WebView.NavigateToString(content);
            _allowDefaultNavigation = false;
        }

        #endregion 

        #region Private Events

        private void WebView_OnFrameNavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            args.Cancel = (_allowDefaultNavigation == false);
        }

        private void WebView_OnNavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            args.Cancel = (_allowDefaultNavigation == false);
        }

        private void WebView_ScriptNotify(object sender, NotifyEventArgs e)
        {
            if (OnLinkClicked != null)
                OnLinkClicked(this, e);
        }

        #endregion Private Events        
    }
}
