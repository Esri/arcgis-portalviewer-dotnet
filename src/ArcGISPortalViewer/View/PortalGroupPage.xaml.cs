// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved

using ArcGISPortalViewer.Common;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace ArcGISPortalViewer.View
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class PortalGroupPage : LayoutAwarePage
    {
        public PortalGroupPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        private void OverviewButton_Click(object sender, RoutedEventArgs e)
        {
            OverviewGrid.Visibility = Visibility.Visible;
            CommentsGrid.Visibility = Visibility.Collapsed;
            OverviewButton.Foreground = new SolidColorBrush(Windows.UI.Colors.Black);
            CommentsButton.Foreground = new SolidColorBrush(Windows.UI.Colors.Gray);
        }

        private void CommentsButton_Click(object sender, RoutedEventArgs e)
        {
            OverviewGrid.Visibility = Visibility.Collapsed;
            CommentsGrid.Visibility = Visibility.Visible;
            CommentsButton.Foreground = new SolidColorBrush(Windows.UI.Colors.Black);
            OverviewButton.Foreground = new SolidColorBrush(Windows.UI.Colors.Gray);
        }
    }
}
