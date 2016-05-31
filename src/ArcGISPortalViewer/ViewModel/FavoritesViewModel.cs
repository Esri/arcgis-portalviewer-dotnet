// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see https://opensource.org/licenses/ms-pl for details.
// All other rights reserved

using Esri.ArcGISRuntime.Portal;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using ArcGISPortalViewer.Helpers;
using ArcGISPortalViewer.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ArcGISPortalViewer.ViewModel
{
    public class FavoritesViewModel : ViewModelBase
    {
        /// <summary>
        /// The <see cref="Items" /> property's name.
        /// </summary>
        public const string ItemsPropertyName = "Items";

        private ObservableCollection<ArcGISPortalItem> _items = null;      

        /// <summary>
        /// Sets and gets the Items property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<ArcGISPortalItem> Items
        {
            get
            {                
                return _items;
            }
            set
            {
                Set(ItemsPropertyName, ref _items, value);
            }
        }
        
        //public string Title { get; set; }
        /// <summary>
        /// The <see cref="Title" /> property's name.
        /// </summary>
        public const string TitlePropertyName = "Title";

        private string _title = "";

        /// <summary>
        /// Sets and gets the Title property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                Set(TitlePropertyName, ref _title, value);
            }
        }

        private ArcGISPortalItem _selectedPortalItem;
        /// <summary>
        /// The <see cref="SelectedItem" /> property's name.
        /// </summary>
        public const string SelectedPortalItemPropertyName = "SelectedPortalItem";

        /// <summary>
        /// Sets and gets the SelectedPortalItem property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ArcGISPortalItem SelectedPortalItem
        {
            get
            {
                return _selectedPortalItem;
            }
            set
            {
                if (Set(SelectedPortalItemPropertyName, ref _selectedPortalItem, value)
                    && value != null)
                {
                    _selectedPortalItem = value;
                }
            }
        }

        public bool IsSelectedItemInFavorites
        {
            get
            {
                if (_selectedPortalItem == null || Items == null)
                    return false;
                return Items.Any(pivm => pivm.Id == _selectedPortalItem.Id);
            }
        }

        private RelayCommand _addToFavoritesCommand;
        public RelayCommand AddToFavoritesCommand
        {
            get
            {
                return _addToFavoritesCommand ?? (_addToFavoritesCommand = new RelayCommand(() =>
                {
                    FavoritesService.CurrentFavoritesService.AddToFavorites(SelectedPortalItem);
                    RaisePropertyChanged(() => IsSelectedItemInFavorites);
                }));
            }
        }

        private RelayCommand _removeFromFavoritesCommand;
        public RelayCommand RemoveFromFavoritesCommand
        {
            get
            {
                return _removeFromFavoritesCommand ?? (_removeFromFavoritesCommand = new RelayCommand(() =>
                {
                    FavoritesService.CurrentFavoritesService.RemoveFromFavorites(SelectedPortalItem);
                    RaisePropertyChanged(() => IsSelectedItemInFavorites);                    
                }));
            }
        }

        public FavoritesViewModel()
        {
            var _ = Initialize();
        }

        private async Task Initialize()
        {
            if (FavoritesService.CurrentFavoritesService.Favorites == null)
            {
                if (await FavoritesService.CurrentFavoritesService.SetFavoritesCollection())
                    Items = FavoritesService.CurrentFavoritesService.Favorites;
            }

            Messenger.Default.Register<AddItemToFavoritesMessage>(this, msg =>
            {
                try
                {
                    FavoritesService.CurrentFavoritesService.AddToFavorites(msg.Item);
                    Items = FavoritesService.CurrentFavoritesService.Favorites;
                    RaisePropertyChanged(() => IsSelectedItemInFavorites);
                }
                catch (Exception ex)
                {
                    var _ = App.ShowExceptionDialog(ex);
                }
            });


            Messenger.Default.Register<RemoveItemFromFavoritesMessage>(this, msg =>
            {
                try
                {
                    FavoritesService.CurrentFavoritesService.RemoveFromFavorites(msg.Item);
                    Items = FavoritesService.CurrentFavoritesService.Favorites;
                    RaisePropertyChanged(() => IsSelectedItemInFavorites);
                }
                catch (Exception ex)
                {
                    var _ = App.ShowExceptionDialog(ex);
                }
            });

            // reset Favorites when signing in
            Messenger.Default.Register<ChangeSignInMessage>(this, msg => { FavoritesService.CurrentFavoritesService.Favorites = null; });
            
            // reset Favorites when signing out
            Messenger.Default.Register<ChangeSignOutMessage>(this, msg => { FavoritesService.CurrentFavoritesService.Favorites = null; });
        }
    }
}
