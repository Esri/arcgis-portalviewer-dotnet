using Esri.ArcGISRuntime.Portal;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using ArcGISPortalViewer.Helpers;
using ArcGISPortalViewer.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace ArcGISPortalViewer.ViewModel
{
    public class PortalCollectionViewModel : ViewModelBase
    {
        public string CollectionTitle { get; private set; }
        
        /// <summary>
        /// The <see cref="CurrentCollection" /> property's name.
        /// </summary>
        public const string CurrentCollectionPropertyName = "CurrentCollection";

        private object _currentCollection = null;

        /// <summary>
        /// Sets and gets the CurrentCollection property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public object CurrentCollection
        {
            get
            {
                return _currentCollection;
            }
            set
            {
                Set(CurrentCollectionPropertyName, ref _currentCollection, value);
                SelectedPortalItem = null; 
            }
        }
        
        public RelayCommand<object> ItemClickCommand { get; set; }

        private ArcGISPortalItem _selectedPortalItem;
        public ArcGISPortalItem SelectedPortalItem 
        { 
            get { return _selectedPortalItem; }
            set
            {
                if (_selectedPortalItem != value)
                {
                    _selectedPortalItem = value;
                    AppViewModel.CurrentAppViewModel.SelectedPortalItem = _selectedPortalItem;
                    RaisePropertyChanged("SelectedPortalItem");
                }
            }
        }
        public IncremetalLoadingCollection PortalItems { get; private set; }
        public ObservableCollection<ArcGISPortalItem> MyMapsItems { get; private set; }
        public ObservableCollection<ArcGISPortalGroup> PortalGroups { get; private set; }
        public ObservableCollection<ArcGISPortalItem> FavoritesItems { get; private set; }

        public PortalCollectionViewModel(INavigationService navigationService)
        {
            Messenger.Default.Register<ChangeIncremetalCollectionMessage>(this, msg =>
            {
                try
                {
                    PortalItems = msg.ItemCollection;
                    CollectionTitle = PortalItems.Title;
                    CurrentCollection = PortalItems;
                }
                catch (Exception ex)
                {
                    var _ = App.ShowExceptionDialog(ex);
                }

            });

            Messenger.Default.Register<ChangePortalItemsCollectionMessage>(this, msg =>
            {
                try
                {
                    MyMapsItems = msg.ItemCollection;
                    CollectionTitle = msg.Title;
                    CurrentCollection = MyMapsItems;
                }
                catch (Exception ex)
                {
                    var _ = App.ShowExceptionDialog(ex);
                }

            });

            Messenger.Default.Register<ChangeFavoritesCollectionMessage>(this, msg =>
            {
                try
                {
                    CollectionTitle = msg.Title;
                    CurrentCollection = FavoritesService.CurrentFavoritesService.Favorites;
                }
                catch (Exception ex)
                {
                    var _ = App.ShowExceptionDialog(ex);
                }

            });

            Messenger.Default.Register<ChangePortalGroupsCollectionMessage>(this, msg =>
            {
                try
                {
                    PortalGroups = msg.ItemCollection;
                    CollectionTitle = "Groups";
                    CurrentCollection = PortalGroups;
                }
                catch (Exception ex)
                {
                    var _ = App.ShowExceptionDialog(ex);
                }

            });

            // initialize ItemClick RelayCommand
            ItemClickCommand = new RelayCommand<object>((e) =>
            {
                // handle the type of EventArgs passed
                if (e == null) return;

                ArcGISPortalItem portalItem = null;
                if (e.GetType() == typeof(ArcGISPortalItem))
                    portalItem = e as ArcGISPortalItem;
                // the GridView sends ItemClickEventArgs
                else if (e.GetType() == typeof(ItemClickEventArgs))
                    portalItem = ((ItemClickEventArgs)e).ClickedItem as ArcGISPortalItem;
                else if (e.GetType() == typeof(PointerRoutedEventArgs))
                    portalItem = ((PointerRoutedEventArgs)e).OriginalSource as ArcGISPortalItem;

                if (portalItem != null)
                {
                    //SelectedPortalItem = portalItem;                    

                    // send clicked item via a message to other ViewModels who
                    // are registered with ChangeItemSelectedMessage
                    Messenger.Default.Send<ChangeItemSelectedMessage>(new ChangeItemSelectedMessage() { Item = portalItem });

                    // use the navigation service to navigate to the page showing the item details                    
                    navigationService.Navigate(App.ItemPageName, portalItem);
                }
                else // check if it is a PortalGroup
                {
                    ArcGISPortalGroup portalGroup = null;
                    // the GridView sends ItemClickEventArgs
                    if (e.GetType() == typeof(ItemClickEventArgs))
                        portalGroup = ((ItemClickEventArgs)e).ClickedItem as ArcGISPortalGroup;

                    if (portalGroup != null)
                    {
                        // send clicked item via a message to other ViewModels who
                        // are registered with ChangeGroupSelectedMessage
                        Messenger.Default.Send<ChangeGroupSelectedMessage>(new ChangeGroupSelectedMessage() { Group = portalGroup });
                        //Use the navigation service to navigate to the page showing the item details                        
                        navigationService.Navigate(App.GroupPageName);
                    }
                }
            });
        }
    }
}
