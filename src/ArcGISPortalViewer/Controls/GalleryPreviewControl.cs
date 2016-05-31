// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see https://opensource.org/licenses/ms-pl for details.
// All other rights reserved

using Callisto.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISPortalViewer.Controls
{
	/// <summary>
	/// Control used on the front page to show a limited set of items,
	/// and in case of overflow, show a live tile + more... tile.
	/// </summary>
	public class GalleryPreviewControl : Panel
	{
		public int currentRowCount = 0;
		public int currentColumnCount = 0;

		/// <summary>
		/// Initializes a new instance of the <see cref="GalleryPreviewControl" /> class.
		/// </summary>
		public GalleryPreviewControl()
		{
			HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;
			VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch;
			Transitions = new Windows.UI.Xaml.Media.Animation.TransitionCollection();
			Transitions.Add(new Windows.UI.Xaml.Media.Animation.RepositionThemeTransition());
		}

		/// <summary>
		/// Provides the behavior for the Measure pass of the layout cycle. Classes can override this method to define
		/// their own Measure pass behavior.
		/// </summary>
		/// <param name="availableSize">The available size that this object can give to child objects. Infinity can be
		/// specified as a value to indicate that the object will size to whatever content is available.</param>
		/// <returns>
		/// The size that this object determines it needs during layout, based on its calculations of the allocated 
		/// sizes for child objects or based on other considerations such as a fixed container size.
		/// </returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			if (ItemsSource == null)
				return new Size(0, 0);
			var list = (ItemsSource as IEnumerable).OfType<object>().ToList();
			int count = list.Count;
			CalculateRowColumnCount(availableSize, count);

			foreach (var item in Children)
			{
				if (item is Callisto.Controls.LiveTile)
					item.Measure(new Size(ColumnWidth * 2, RowHeight * 2));
				else
					item.Measure(new Size(ColumnWidth, RowHeight));
			}

			//If this is an incremental loading datasource, see if we can get enough to fill up and show more... tile.
			if (ItemsSource is Windows.UI.Xaml.Data.ISupportIncrementalLoading)
			{
                var maxTileSpace = Math.Min(MaxColumnCount, Math.Ceiling(availableSize.Width / ColumnWidth)) * Math.Ceiling(availableSize.Height / RowHeight);
				if (count <= maxTileSpace)
				{
					var iloading = (ItemsSource as Windows.UI.Xaml.Data.ISupportIncrementalLoading);
					if (iloading.HasMoreItems)
					{
						var loadmoreTask = iloading.LoadMoreItemsAsync((uint)maxTileSpace + 1);
					}
				}
			}

			return new Size(currentColumnCount * ColumnWidth, currentRowCount * RowHeight);
		}

		/// <summary>
		/// Provides the behavior for the Arrange pass of layout. Classes can override this method to define their own Arrange pass behavior.
		/// </summary>
		/// <param name="finalSize">The final area within the parent that this object should use to arrange itself and its children.</param>
		/// <returns>
		/// The actual size that is used after the element is arranged in layout.
		/// </returns>
		protected override Size ArrangeOverride(Size finalSize)
		{
			if (ItemsSource == null)
				return new Size(0, 0);
			
			var list = (ItemsSource as IEnumerable).OfType<object>().ToList();
			int count = list.Count;
			CalculateRowColumnCount(finalSize, count);
			int MaxChildCount = currentRowCount * currentColumnCount;
			bool hasLiveTile = count > MaxChildCount;

			if (Children.Count != Math.Min(currentRowCount * currentColumnCount - (hasLiveTile ? 3 : 0), count))
			{
				RebuildTable();
			}

			int i = 0;
			int row = 0;
			int column = 0;
			hasLiveTile = false;
			foreach (var item in Children)
			{
				if (item is LiveTile && i == 0)
				{
					hasLiveTile = true;
					item.Arrange(new Rect(0, 0, ColumnWidth * 2, RowHeight * 2));
					row++;
				}
				else
				{
					item.Arrange(new Rect(column * ColumnWidth, row * RowHeight, ColumnWidth, RowHeight));
				}
				row++;
				if (row >= currentRowCount)
				{
					column++;
					if (hasLiveTile && column == 1)
						row = 2;
					else
						row = 0;
					if (column >= currentColumnCount)
						break;
				}
			}
			return new Size(currentColumnCount * ColumnWidth, currentRowCount * RowHeight);
		}

		private void CalculateRowColumnCount(Size availableSize, int count)
		{
			//Calculate how many rows/cols we need for the number of items
			if (!double.IsPositiveInfinity(availableSize.Width))
				currentColumnCount = (int)Math.Floor(availableSize.Width / ColumnWidth);
			else
				currentColumnCount = MaxColumnCount;
			currentRowCount = (int)Math.Floor(availableSize.Height / RowHeight);
			if (count > 0)
				currentColumnCount = (int)Math.Min(Math.Ceiling(count / (double)currentRowCount), MaxColumnCount);
			else
				currentColumnCount = 0;
			if (count < currentRowCount) currentRowCount = count;
		}

		private void RebuildTable()
		{
			Children.Clear();
			if (ItemsSource == null || !(ItemsSource is IEnumerable))
				return;
			var list = (ItemsSource as IEnumerable).OfType<object>().ToList();
			int count = list.Count;
			int rows = currentRowCount;
			int cols = currentColumnCount;
			int itemSpace = rows * cols;
			bool hasLiveTile = false;
			if (count > itemSpace)
			{
				LiveTile tile = new LiveTile()
				{
					ItemsSource = ItemsSource,
					ItemTemplate = LiveTileItemTemplate,
					Margin = new Thickness(0, 0, 20, 20)
				};
				tile.SetValue(Callisto.Effects.Tilt.IsTiltEnabledProperty, true);
				Children.Add(tile);
				hasLiveTile = true;
				tile.Tapped += livetile_Tapped;
			}
			int i = 0;
			for (int c = 0; c < cols; c++)
			{
				for (int r = 0; r < rows; r++)
				{
					if (i >= list.Count)
						break;
					if (hasLiveTile && c < 2 && r < 2)
						continue;
					ContentControl ctrl = new ContentControl()
					{
						Content = list[i++],
						ContentTemplate = ItemTemplate,
						Margin = new Thickness(0, 0, 20, 20),
						HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch,
						VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch, 
						HorizontalContentAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch,
						VerticalContentAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch
					};
					//ctrl.Transitions = new Windows.UI.Xaml.Media.Animation.TransitionCollection();
					//ctrl.Transitions.Add(new Windows.UI.Xaml.Media.Animation.AddDeleteThemeTransition());
					//ctrl.Transitions.Add(new Windows.UI.Xaml.Media.Animation.EntranceThemeTransition());
					ctrl.SetValue(Callisto.Effects.Tilt.IsTiltEnabledProperty, true);
					if (r == rows - 1 && c == Math.Min(cols, MaxColumnCount) - 1 &&
						count > rows * cols) // last item
					{
						ctrl.ContentTemplate = MoreTemplate;
						ctrl.Tapped += moreTile_Tapped;
					}
					else
					{
						ctrl.Tapped += ctrl_Tapped;
					}
					Children.Add(ctrl);
				}
			}
		}

		private void livetile_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
		{
			var item = (sender as LiveTile).GetCurrent();
            if (ItemClick != null && item != null)
                ItemClick(this, new TileClickEventArgs(item));
		}

		private void ctrl_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
		{
            var item = (sender as ContentControl).Content;
			if (ItemClick != null)
                ItemClick(this, new TileClickEventArgs(item));
		}

		private void moreTile_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
		{
            var item = (sender as ContentControl).Content;
			if (MoreClicked != null)
                MoreClicked(this, new TileClickEventArgs(item));
		}

		public int MaxColumnCount
		{
			get { return (int)GetValue(MaxColumnCountProperty); }
			set { SetValue(MaxColumnCountProperty, value); }
		}

		public static readonly DependencyProperty MaxColumnCountProperty =
			DependencyProperty.Register("MaxColumnCount", typeof(int), typeof(GalleryPreviewControl), new PropertyMetadata(3));

		public double RowHeight
		{
			get { return (double)GetValue(RowHeightProperty); }
			set { SetValue(RowHeightProperty, value); }
		}

		public static readonly DependencyProperty RowHeightProperty =
			DependencyProperty.Register("RowHeight", typeof(double), typeof(GalleryPreviewControl), new PropertyMetadata(100d));

		public double ColumnWidth
		{
			get { return (double)GetValue(ColumnWidthProperty); }
			set { SetValue(ColumnWidthProperty, value); }
		}

		public static readonly DependencyProperty ColumnWidthProperty =
			DependencyProperty.Register("ColumnWidth", typeof(double), typeof(GalleryPreviewControl), new PropertyMetadata(200d));

		public object ItemsSource
		{
			get { return (object)GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}

		public static readonly DependencyProperty ItemsSourceProperty =
			DependencyProperty.Register("ItemsSource", typeof(object), typeof(GalleryPreviewControl), new PropertyMetadata(null, OnItemsSourcePropertyChanged));

		private static void OnItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var ctrl = d as GalleryPreviewControl;
			if (e.OldValue != null)
			{
				if (e.OldValue is INotifyCollectionChanged)
				{
					(e.OldValue as INotifyCollectionChanged).CollectionChanged -= ctrl.GalleryPreviewControl_CollectionChanged;
				}
			}

			if (e.NewValue != null)
			{
				if (e.NewValue is INotifyCollectionChanged)
				{
					(e.NewValue as INotifyCollectionChanged).CollectionChanged += ctrl.GalleryPreviewControl_CollectionChanged;
				}
			}
			ctrl.Children.Clear();
			ctrl.InvalidateMeasure();
		}

		private void GalleryPreviewControl_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			InvalidateMeasure();
			return;
		}

		public DataTemplate ItemTemplate
		{
			get { return (DataTemplate)GetValue(ItemTemplateProperty); }
			set { SetValue(ItemTemplateProperty, value); }
		}

		public static readonly DependencyProperty ItemTemplateProperty =
			DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(GalleryPreviewControl), new PropertyMetadata(null));

		public DataTemplate LiveTileItemTemplate
		{
			get { return (DataTemplate)GetValue(LiveTileItemTemplateProperty); }
			set { SetValue(LiveTileItemTemplateProperty, value); }
		}

		public static readonly DependencyProperty LiveTileItemTemplateProperty =
			DependencyProperty.Register("LiveTileItemTemplate", typeof(DataTemplate), typeof(GalleryPreviewControl), new PropertyMetadata(null));

		public DataTemplate MoreTemplate
		{
			get { return (DataTemplate)GetValue(MoreTemplateProperty); }
			set { SetValue(MoreTemplateProperty, value); }
		}

		public static readonly DependencyProperty MoreTemplateProperty =
			DependencyProperty.Register("MoreTemplate", typeof(DataTemplate), typeof(GalleryPreviewControl), new PropertyMetadata(null));

        public event EventHandler<TileClickEventArgs> ItemClick;
        public event EventHandler<TileClickEventArgs> MoreClicked;
	}

    public sealed class TileClickEventArgs : RoutedEventArgs
    {
        internal TileClickEventArgs(object item) //, bool isMoreClick = false)
        {
            ClickedTile = item;
            //IsMoreClick = isMoreClick;
        }

        public object ClickedTile { get; private set; }
        //public bool IsMoreClick { get; private set; }
    }
}
