using Windows.ApplicationModel.Store;
using ArcGISPortalViewer.ViewModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace ArcGISPortalViewer.Controls
{
    public sealed partial class IdentifyResultsControl
    {
        private const string SELECTED_NUMBER_OUT_OF_TOTAL_ITEMS_FORMAT_STRING = "({0} of {1})";        

        public IdentifyResultsControl()
        {
            InitializeComponent();
        }        

        #region ItemsSource

        public IEnumerable<PopupItem> ItemsSource
        {
            get { return (IEnumerable<PopupItem>)GetValue(ItemsSourceProperty); }
            set{ SetValue(ItemsSourceProperty, value);}
        }
        
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(object), typeof(IdentifyResultsControl), new PropertyMetadata(null, OnItemsSourcePropertyChanged));

        private static void OnItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {            
            var control = ((IdentifyResultsControl)d);            
            control.UpdateCount();
            control.UpdateSelectedItemPositionText();
        }        

        #endregion ItemsSource

        #region SelectedItem

        public PopupItem SelectedItem
        {
            get { return (PopupItem)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(PopupItem), typeof(IdentifyResultsControl), new PropertyMetadata(null, OnSelectedItemPropertyChanged));

        private static void OnSelectedItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = ((IdentifyResultsControl) d);            
            control.UpdateSelectedItemPositionText();
        }

        #endregion SelectedItem

        #region SelectedItemPositionText

        public string SelectedItemPositionText
        {
            get { return (string)GetValue(SelectedItemPositionTextProperty); }
            private set { SetValue(SelectedItemPositionTextProperty, value); }
        }
        
        public static readonly DependencyProperty SelectedItemPositionTextProperty =
            DependencyProperty.Register("SelectedItemPositionText", typeof(string), typeof(IdentifyResultsControl), new PropertyMetadata(null));

        #endregion SelectedItemPositionText

        #region Count

        public int Count
        {
            get { return (int)GetValue(CountProperty); }
            private set { SetValue(CountProperty, value); }
        }

        public static readonly DependencyProperty CountProperty =
            DependencyProperty.Register("Count", typeof(int), typeof(IdentifyResultsControl), new PropertyMetadata(0));

        #endregion Count

        #region ShowDetailView

        public bool ShowDetailView
        {
            get { return (bool)GetValue(ShowDetailViewProperty); }
            set { SetValue(ShowDetailViewProperty, value); }
        }
        
        public static readonly DependencyProperty ShowDetailViewProperty =
            DependencyProperty.Register("ShowDetailView", typeof(bool), typeof(IdentifyResultsControl), new PropertyMetadata(false));        

        #endregion ShowDetailView


        #region OnSelectedItemClickedCommand

        public ICommand OnSelectedItemClickedCommand
        {
            get { return (ICommand)GetValue(OnSelectedItemClickedCommandProperty); }
            set { SetValue(OnSelectedItemClickedCommandProperty, value); }
        }
        
        public static readonly DependencyProperty OnSelectedItemClickedCommandProperty =
            DependencyProperty.Register("OnSelectedItemClickedCommand", typeof(ICommand), typeof(IdentifyResultsControl), new PropertyMetadata(null));

        #endregion OnSelectedItemClickedCommand

        #region OnSetViewClickedCommand

        public ICommand OnSetViewClickedCommand
        {
            get { return (ICommand)GetValue(OnSetViewClickedCommandProperty); }
            set { SetValue(OnSetViewClickedCommandProperty, value); }
        }
        
        public static readonly DependencyProperty OnSetViewClickedCommandProperty =
            DependencyProperty.Register("OnSetViewClickedCommand", typeof(ICommand), typeof(IdentifyResultsControl), new PropertyMetadata(null));

        #endregion OnSetViewClickedCommand

        #region OnBackClickedCommand

        public ICommand OnBackClickedCommand
        {
            get { return (ICommand)GetValue(OnBackClickedCommandProperty); }
            set { SetValue(OnBackClickedCommandProperty, value); }
        }
        
        public static readonly DependencyProperty OnBackClickedCommandProperty =
            DependencyProperty.Register("OnBackClickedCommand", typeof(ICommand), typeof(IdentifyResultsControl), new PropertyMetadata(null));

        #endregion OnBackClickedCommand

        #region Private Methods

        private void UpdateSelectedItemPositionText()
        {            
            var total = ItemsSource != null ? ItemsSource.Count() : 0;
            var position = ItemsSource != null && SelectedItem != null ? (ItemsSource.ToList().IndexOf(SelectedItem) + 1) : 0;
            SelectedItemPositionText = string.Format(SELECTED_NUMBER_OUT_OF_TOTAL_ITEMS_FORMAT_STRING, position, total );
        }

        private void UpdateCount()
        {
            Count = ItemsSource != null ? ItemsSource.Count() : 0;
            FeturesFoundTextBlock.Text = Count == 1 ? "1 feature found" : string.Format("{0} features found", Count);
        }

        #endregion Private Methods
    }
}
