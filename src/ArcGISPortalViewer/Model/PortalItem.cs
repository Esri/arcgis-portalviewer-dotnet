// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved

using System;
using Esri.ArcGISRuntime.Portal;
using GalaSoft.MvvmLight;
using Windows.UI.Xaml.Media.Imaging;

namespace ArcGISPortalViewer.Model
{

    public class PortalItem : ObservableObject
    {
        public ArcGISPortalItem ArcGISPortalItem { get; private set; }

#if DEBUG
        public PortalItem(string title, BitmapImage image)
        {
            _title = title;
            _image = image;
        }
#endif

        public PortalItem(ArcGISPortalItem item)
        {
            ArcGISPortalItem = item;
        }

        private string _title = string.Empty;

        public string Title
        {
            get
            {
                if (ArcGISPortalItem == null)
                    return _title;
                return ArcGISPortalItem.Title;
            }
            private set { _title = value; }
        }

        private BitmapImage _image = null;

        public BitmapImage Image
        {
            get
            {
                if (_image == null && ArcGISPortalItem != null)
                    _image = ArcGISPortalItem.ThumbnailUri == null
                        ? new BitmapImage()
                        : new BitmapImage(ArcGISPortalItem.ThumbnailUri);
                return _image;
            }
            private set { _image = value; }
        }

        private Uri _thumbnailUri = null;

        public Uri ThumbnailUri
        {
            get
            {
                if (Image != null && Image is BitmapImage)
                    return ((BitmapImage)Image).UriSource;
                return null;

                //if (ArcGISPortalItem == null)                   
                //    return _thumbnailUri;
                //return ArcGISPortalItem.ThumbnailUri;
            }
            private set { _thumbnailUri = value; }
        }
    }
}
