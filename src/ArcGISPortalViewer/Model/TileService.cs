// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see https://opensource.org/licenses/ms-pl for details.
// All other rights reserved

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;

namespace ArcGISPortalViewer.Model
{
    public static class TileService
    {
        public static void ClearTileNotification()
        {
            // the same TileUpdateManager can be used to clear the tile since   
            // tile notifications are being sent to the application's default tile  
            TileUpdateManager.CreateTileUpdaterForApplication().Clear();
        }

        public static void SendTileTextNotification(string text)
        {
            SendWideTileTextNotification(text);
            SendSmallTileTextNotification(text);
        }

        public static void SendWideTileTextNotification(string text, string imageUri = "Assets/WideLogo.jpg")
        {
            // Get a filled in version of the template by using getTemplateContent  
            var tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150PeekImageAndText01);

            // You will need to look at the template documentation to know how many text fields a particular template has         

            // get the text attributes for this template and fill them in  
            var tileAttributes = tileXml.GetElementsByTagName("text");
            tileAttributes[0].AppendChild(tileXml.CreateTextNode(text));

            var tileAttributes2 = tileXml.GetElementsByTagName("image");
            tileAttributes2[0].Attributes[1].InnerText = imageUri;

            // create the notification from the XML  
            var tileNotification = new TileNotification(tileXml);

            // send the notification to the app's default tile  
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
        }

        public static void SendSmallTileTextNotification(string text, string imageUri = "Assets/WideLogo.jpg")
        {
            // Get a filled in version of the template by using getTemplateContent  
            var tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150PeekImageAndText01);

            // You will need to look at the template documentation to know how many text fields a particular template has         

            // get the text attributes for this template and fill them in  
            var tileAttributes = tileXml.GetElementsByTagName("text");
            tileAttributes[0].AppendChild(tileXml.CreateTextNode(text));

            var tileAttributes2 = tileXml.GetElementsByTagName("image");
            tileAttributes2[0].Attributes[1].InnerText = imageUri;

            // create the notification from the XML  
            var tileNotification = new TileNotification(tileXml);

            // send the notification to the app's default tile  
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
        }


        public async static Task CreateSecondaryTileFromWebImage(string title, string id, Uri image, Rect TileProptRect, string navigateUri)
        {
            try
            {
                string filename = string.Format("{0}.png", id);
                //download thumb
                HttpClient httpClient = new HttpClient();
                var response = await httpClient.GetAsync(image);
                var imageFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
                using (var fs = await imageFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    using (var outStream = fs.GetOutputStreamAt(0))
                    {
                        DataWriter writer = new DataWriter(outStream);
                        writer.WriteBytes(await response.Content.ReadAsByteArrayAsync());
                        await writer.StoreAsync();
                        writer.DetachStream();
                        await outStream.FlushAsync();
                    }
                }
                await DarkenImageBottom(filename, filename); //in-place replacement of downloaded image
                Uri logo = new Uri(string.Format("ms-appdata:///local/{0}", filename));
                CreateSecondaryTile(title, id, logo, TileProptRect, navigateUri);
            }
            catch
            {
                //oops
            }
        }

        public async static void CreateSecondaryTile(string title, string id, Uri image, Rect TileProptRect, string navigateUri)
        {
            SecondaryTile secondaryTile = new SecondaryTile(id, title, navigateUri, image, Windows.UI.StartScreen.TileSize.Square150x150);
            secondaryTile.VisualElements.ShowNameOnSquare150x150Logo = true;
            secondaryTile.VisualElements.ForegroundText = ForegroundText.Light;
            await secondaryTile.RequestCreateForSelectionAsync(TileProptRect, Windows.UI.Popups.Placement.Above);
        }

        /// <summary>
        /// Generates a darker bottom on an image
        /// </summary>
        /// <param name="filename">filename in LocalFolder</param>
        /// <param name="outfilename">filename in LocalFolder</param>
        /// <returns>Awaitable Task</returns>
        private async static Task DarkenImageBottom(string filename, string outfilename)
        {
            var file = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFileAsync(filename);
            BitmapDecoder decoder = null;
            byte[] sourcePixels = null;
            using (IRandomAccessStream fileStream = await file.OpenReadAsync())
            {
                decoder = await BitmapDecoder.CreateAsync(fileStream);
                // Scale image to appropriate size 
                BitmapTransform transform = new BitmapTransform();
                PixelDataProvider pixelData = await decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8, // WriteableBitmap uses BGRA format 
                    BitmapAlphaMode.Straight,
                    transform,
                    ExifOrientationMode.IgnoreExifOrientation, // This sample ignores Exif orientation 
                    ColorManagementMode.DoNotColorManage
                );
                // An array containing the decoded image data, which could be modified before being displayed 
                sourcePixels = pixelData.DetachPixelData();
                fileStream.Dispose();
            }

            for (uint col = 0; col < decoder.PixelWidth; col++)
            {
                for (uint row = 0; row < decoder.PixelHeight; row++)
                {
                    if (row < decoder.PixelHeight * .6) continue;
                    uint idx = (row * decoder.PixelWidth + col) * 4;
                    if (decoder.BitmapPixelFormat == BitmapPixelFormat.Bgra8 ||
                        decoder.BitmapPixelFormat == BitmapPixelFormat.Rgba8)
                    {
                        var frac = 1 - Math.Sin(((row / (double)decoder.PixelHeight) - .6) * (1 / .4));
                        byte b = sourcePixels[idx];
                        byte g = sourcePixels[idx + 1];
                        byte r = sourcePixels[idx + 2];
                        sourcePixels[idx] = (byte)(b * frac);
                        sourcePixels[idx + 1] = (byte)(g * frac);
                        sourcePixels[idx + 2] = (byte)(r * frac);
                    }
                }
            }

            var file2 = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync(outfilename, CreationCollisionOption.ReplaceExisting);

            var str = await file2.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
            BitmapEncoder enc = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, str);
            enc.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, decoder.PixelWidth, decoder.PixelHeight,
                decoder.DpiX, decoder.DpiY, sourcePixels);
            await enc.FlushAsync();
            str.Dispose();
        }
    }
}
