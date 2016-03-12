// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved

/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocatorTemplate xmlns:vm="using:ProjectForTemplates.ViewModel"
                                   x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
*/

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using ArcGISPortalViewer.Model;
using ArcGISPortalViewer.Helpers;

namespace ArcGISPortalViewer.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            if (ViewModelBase.IsInDesignModeStatic)
            {
                SimpleIoc.Default.Register<IPortalService>(() => new Design.DesignPortalService());
                SimpleIoc.Default.Register<INavigationService, Design.DesignNavigationService>();
            }
            else
            {
                SimpleIoc.Default.Register<IPortalService, PortalService>();
                SimpleIoc.Default.Register<INavigationService>(() => new NavigationService());
                SimpleIoc.Default.Register<IFavoritesService, FavoritesService>();
                SimpleIoc.Default.Register<IncremetalLoadingCollection, IncremetalLoadingCollection>();
            }

            SimpleIoc.Default.Register<AppViewModel>(true);
            SimpleIoc.Default.Register<ArcGISBlankViewModel>(); //created up front to registers login messages when signing is triggered            
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<PortalItemViewModel>(true);
            SimpleIoc.Default.Register<PortalGroupViewModel>(true);
            SimpleIoc.Default.Register<PortalCollectionViewModel>(true);
            SimpleIoc.Default.Register<FavoritesViewModel>();
            SimpleIoc.Default.Register<SearchViewModel>(true);
        }

        /// <summary>
        /// Gets the AppVM property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public AppViewModel AppVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<AppViewModel>();
            }
        }

        /// <summary>
        /// Gets the ArcGISBlankVMLocator property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public ArcGISBlankViewModel ArcGISBlankVMLocator
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ArcGISBlankViewModel>();
            }
        }

        /// <summary>
        /// Gets the Main property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

        /// <summary>
        /// Gets the PortalItem property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public PortalItemViewModel PortalItemVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<PortalItemViewModel>();
            }
        }

        /// <summary>
        /// Gets the PortalGroupVM property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public PortalGroupViewModel PortalGroupVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<PortalGroupViewModel>();
            }
        }

        /// <summary>
        /// Gets the PortalItemsCollection property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public PortalCollectionViewModel PortalItemsCollection
        {
            get
            {
                return ServiceLocator.Current.GetInstance<PortalCollectionViewModel>();
            }
        }

        /// <summary>
        /// Gets the SearchVM property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public SearchViewModel SearchVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SearchViewModel>();
            }
        }

        /// <summary>
        /// Cleans up all the resources.
        /// </summary>
        public static void Cleanup()
        {
        }
    }
}