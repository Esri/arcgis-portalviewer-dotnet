// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved

using Esri.ArcGISRuntime.Portal;
using ArcGISPortalViewer.Model;
using ArcGISPortalViewer.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using GalaSoft.MvvmLight;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ArcGISPortalViewer.Helpers
{
    public class IncremetalLoadingCollection : ObservableCollection<object>, ISupportIncrementalLoading
    {
        public delegate Task<IEnumerable<object>> getMore(uint count);
        private getMore GetMoreAsync;
        public event EventHandler<bool> IsLoadingDataEventHandler;
        public string Title { get; set; }
        private uint resultcount = 0;

        private bool _isEmpty = true;
        /// <summary>
        /// Sets and gets the IsEmpty property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return _isEmpty;
            }

            set
            {
                if (_isEmpty != value)
                {
                    _isEmpty = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public IncremetalLoadingCollection(getMore getMoreAsync)
        {
            GetMoreAsync = getMoreAsync;
            HasMoreItems = true;
        }

        public bool HasMoreItems { get; private set; }

        public Windows.Foundation.IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return LoadMoreItemsAsyncTask(count).AsAsyncOperation<LoadMoreItemsResult>();
        }

        private async Task<LoadMoreItemsResult> LoadMoreItemsAsyncTask(uint count)
        {
            if (IsLoadingDataEventHandler != null)
                IsLoadingDataEventHandler(this, true);

            List<object> resList = null;
            var result = await GetMoreAsync(count);
            if (result != null)
            {
                resList = new List<object>(result);
                foreach (object item in resList)
                {
                    this.Add(item);
                    resultcount++;
                }
            }

            if (resList == null || resList.Count <= 0)
                HasMoreItems = false;

            if (IsLoadingDataEventHandler != null)
                IsLoadingDataEventHandler(this, false);

            IsEmpty = this.Count() <= 0 && !HasMoreItems;
            return new LoadMoreItemsResult() { Count = resultcount };
        }

        public void Reset()
        {
            this.ClearItems();
            HasMoreItems = true;
            resultcount = 0;
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
    }
}
