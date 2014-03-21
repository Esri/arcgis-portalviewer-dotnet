using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISPortalViewer.Helpers
{
    /// <summary>
    /// ListView Behavior class
    /// </summary>
    public static class ListViewBehavior
    {
        #region IsItemBroughtIntoViewWhenSelected

        /// <summary>
        /// Gets the IsItemBroughtIntoViewWhenSelected value
        /// </summary>
        /// <param name="listView"></param>
        /// <returns></returns>
        public static bool GetIsItemBroughtIntoViewWhenSelected(ListView listView)
        {
            return (bool)listView.GetValue(IsItemBroughtIntoViewWhenSelectedProperty);
        }

        /// <summary>
        /// Sets the IsBroughtIntoViewWhenSelected value
        /// </summary>
        /// <param name="listView"></param>
        /// <param name="value"></param>
        public static void SetIsItemBroughtIntoViewWhenSelected(
          ListView listView, bool value)
        {
            listView.SetValue(IsItemBroughtIntoViewWhenSelectedProperty, value);
        }

        /// <summary>
        /// Determins if the listView is bought into view when enabled
        /// </summary>
        public static readonly DependencyProperty IsItemBroughtIntoViewWhenSelectedProperty =
            DependencyProperty.RegisterAttached(
            "IsItemBroughtIntoViewWhenSelected",
            typeof(bool),
            typeof(ListViewBehavior),
            new PropertyMetadata(false, OnIsItemBroughtIntoViewWhenSelectedChanged));

        /// <summary>
        /// Action to take when item is brought into view
        /// </summary>
        /// <param name="depObj"></param>
        /// <param name="e"></param>
        static void OnIsItemBroughtIntoViewWhenSelectedChanged(
          DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            ListView listView = depObj as ListView;
            if (listView == null)
                return;

            if (e.NewValue is bool == false)
                return;

            if ((bool)e.NewValue)
                listView.SelectionChanged += OnListViewSelectionChanged;
            else
                listView.SelectionChanged -= OnListViewSelectionChanged;
        }

        static void OnListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //// Only react to the Selected event raised by the listView 
            //// whose IsSelected property was modified.  Ignore all ancestors 
            //// who are merely reporting that a descendant's Selected fired. 
            //if (!Object.ReferenceEquals(sender, e.OriginalSource))
            //    return;

            ListView listView = sender as ListView;
            try
            {
                if (listView != null && e.AddedItems.Count()>0)
                    listView.ScrollIntoView(e.AddedItems[0]);
            }
            catch (Exception ex)
            {
                var st = ex;
            }
        }

        #endregion // IsItemBroughtIntoViewWhenSelected
    }
}
