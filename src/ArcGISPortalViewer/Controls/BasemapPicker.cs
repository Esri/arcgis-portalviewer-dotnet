// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved

using Esri.ArcGISRuntime.Portal;
using System;
using System.ComponentModel;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace ArcGISPortalViewer.Controls
{
	public sealed class BasemapPicker : ItemsControl, INotifyPropertyChanged
	{
		public BasemapPicker()
		{
			this.DefaultStyleKey = typeof(BasemapPicker);
		}
		
		protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
		{
			base.PrepareContainerForItemOverride(element, item);
			(element as FrameworkElement).PointerPressed += BasemapGallery_PointerPressed;
		}
		protected override void ClearContainerForItemOverride(DependencyObject element, object item)
		{
			base.ClearContainerForItemOverride(element, item);
			(element as FrameworkElement).PointerPressed -= BasemapGallery_PointerPressed;
		}
		private void BasemapGallery_PointerPressed(object sender, PointerRoutedEventArgs e)
		{
			var item = (sender as FrameworkElement).DataContext as ArcGISPortalItem;
			OnItemClicked(item);
		}
		private ICommand m_OnItemSelectedCommand;
		/// <summary>
		/// Gets the on item clicked command. Used for binding a command into the items template to detect click on an item.
		/// </summary>
		public ICommand OnItemClickedCommand
		{
			get
			{
				if (m_OnItemSelectedCommand == null)
				{
					m_OnItemSelectedCommand = new ClickArcGISPortalItemCommand(this);
				}
				return m_OnItemSelectedCommand;
			}
		}

		/// <summary>
		/// Gets or sets the ArcGISPortal instance to get the basemaps from.
		/// </summary>
		public ArcGISPortal ArcGISPortal
		{
			get { return (ArcGISPortal)GetValue(ArcGISPortalProperty); }
			set { SetValue(ArcGISPortalProperty, value); }
		}

		/// <summary>
		/// Identifies the <see cref="ArcGISPortal"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ArcGISPortalProperty =
			DependencyProperty.Register("ArcGISPortal", typeof(ArcGISPortal),
			typeof(BasemapPicker), new PropertyMetadata(null, OnArcGISPortalPropertyChanged));

		private static void OnArcGISPortalPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			BasemapPicker gallery = (BasemapPicker)d;
			ArcGISPortal oldValue = (ArcGISPortal)e.OldValue;
			if (oldValue != null)
			{
				gallery.ItemsSource = null;
			}
			ArcGISPortal newValue = (ArcGISPortal)e.NewValue;
			if (newValue != null)
			{
				if (newValue.ArcGISPortalInfo != null)
				{
					gallery.RefreshBasemaps();
				}
			}
		}

		private async void RefreshBasemaps()
		{
            if (ArcGISPortal != null && ArcGISPortal.ArcGISPortalInfo != null)
            {
                SearchParameters parameters = new SearchParameters() { Limit = 24 };
                try
                {
                    IsLoadingBasemaps = true;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("IsLoadingBasemaps"));
                    var basemaps = await ArcGISPortal.ArcGISPortalInfo.SearchBasemapGalleryAsync(parameters);
                    ItemsSource = basemaps.Results;
                }
                catch
                {
                    ItemsSource = null;
                }
                IsLoadingBasemaps = false;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("IsLoadingBasemaps"));
                if (ItemsLoaded != null)
                    ItemsLoaded(this, EventArgs.Empty);
            }
            else if (ItemsSource != null)
            {
                ItemsSource = null;
                if (ItemsLoaded != null)
                    ItemsLoaded(this, EventArgs.Empty);
            }
		}

		internal void OnItemClicked(ArcGISPortalItem arcGISPortalItem)
		{
			if (ItemClick != null)
				ItemClick(this, new ArcGISPortalItemClickEventArgs(arcGISPortalItem));
		}

		public event EventHandler<ArcGISPortalItemClickEventArgs> ItemClick;
		public event EventHandler ItemsLoaded;

		private class ClickArcGISPortalItemCommand : ICommand
		{
			BasemapPicker m_owner;
			public ClickArcGISPortalItemCommand(BasemapPicker owner)
			{
				m_owner = owner;
			}
			/// <summary>
			/// Defines the method that determines whether the command can execute in its current state.
			/// </summary>
			/// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
			/// <returns>
			/// true if this command can be executed; otherwise, false.
			/// </returns>
			public bool CanExecute(object parameter)
			{
				return parameter is ArcGISPortalItem && m_owner != null;
			}

			/// <summary>
			/// Occurs when changes occur that affect whether the command should execute.
			/// </summary>
			public event EventHandler CanExecuteChanged;

			/// <summary>
			/// Defines the method to be called when the command is invoked.
			/// </summary>
			/// <param name="parameter">Data used by the command. If the command does not require data to be passed,
			/// this object can be set to null.</param>
			public void Execute(object parameter)
			{
				m_owner.OnItemClicked(parameter as ArcGISPortalItem);
			}
		}

        public bool IsLoadingBasemaps { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public sealed class ArcGISPortalItemClickEventArgs : RoutedEventArgs
		{
			ArcGISPortalItem m_item;
			internal ArcGISPortalItemClickEventArgs(ArcGISPortalItem item)
			{
				m_item = item;
			}
			/// <summary>
			/// Gets the ArcGIS Portal item associated with this event.
			/// </summary>
			public ArcGISPortalItem ArcGISPortalItem { get { return m_item; } }
		}
    }
}