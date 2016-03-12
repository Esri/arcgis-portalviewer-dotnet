﻿// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved

using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Esri.ArcGISRuntime.Location;
using System;

namespace ArcGISPortalViewer.Controls
{
    public sealed partial class LocationDisplayToggle : AppBarButton
    {
        public LocationDisplayToggle()
        {
            this.InitializeComponent();

            try
            {
                var compass = Windows.Devices.Sensors.Compass.GetDefault();
                if (compass != null)
                    CompassItem.Visibility = Visibility.Visible;
            }
            catch { }
            UpdateIcon();
        }

        public LocationDisplay LocationDisplay
        {
            get { return (LocationDisplay)GetValue(LocationDisplayProperty); }
            set { SetValue(LocationDisplayProperty, value); }
        }

        public static readonly DependencyProperty LocationDisplayProperty =
            DependencyProperty.Register("LocationDisplay", typeof(LocationDisplay), typeof(LocationDisplayToggle), new PropertyMetadata(null, OnLocationDisplayPropertyChanged));

        private static void OnLocationDisplayPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (LocationDisplayToggle)d;
            if (e.NewValue != null)
            {
                var b = new Binding()
                {
                    Source = e.NewValue,
                    Path = new PropertyPath("IsEnabled"),
                    Mode = BindingMode.TwoWay
                };
                ctrl.SetBinding(IsLocationEnabledProperty, b);
                b = new Binding()
                {
                    Source = e.NewValue,
                    Path = new PropertyPath("AutoPanMode"),
                    Mode = BindingMode.TwoWay
                };
                ctrl.SetBinding(ModeProperty, b);
            }
            else
            {
                ctrl.SetBinding(IsLocationEnabledProperty, null);
                ctrl.SetBinding(ModeProperty, null);
            }
            ctrl.UpdateIcon();
        }
        private bool IsLocationEnabled
        {
            get { return (bool)GetValue(IsLocationEnabledProperty); }
            set { SetValue(IsLocationEnabledProperty, value); }
        }

        private static readonly DependencyProperty IsLocationEnabledProperty =
            DependencyProperty.Register("IsLocationEnabled", typeof(bool), typeof(LocationDisplay), new PropertyMetadata(false, OnIsLocationEnabledPropertyChanged));

        private static void OnIsLocationEnabledPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (LocationDisplayToggle)d;
            ctrl.UpdateIcon();
        }
        private AutoPanMode Mode
        {
            get { return (AutoPanMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }

        private static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register("Mode", typeof(AutoPanMode), typeof(LocationDisplayToggle),
            new PropertyMetadata(AutoPanMode.Off, OnModePropertyChanged));

        private static void OnModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (LocationDisplayToggle)d;
            ctrl.OnAutoPanModeChanged((AutoPanMode)e.OldValue, (AutoPanMode)e.NewValue);
            ctrl.UpdateIcon();
        }


        private void OffItem_Click(object sender, RoutedEventArgs e)
        {
            IsLocationEnabled = false;
        }

        private void OnItem_Click(object sender, RoutedEventArgs e)
        {
            IsLocationEnabled = true;
            Mode = AutoPanMode.Off;
        }

        private void AutoPanItem_Click(object sender, RoutedEventArgs e)
        {
            IsLocationEnabled = true;
            Mode = AutoPanMode.Default;
        }

        private void CompassItem_Click(object sender, RoutedEventArgs e)
        {
            IsLocationEnabled = true;
            Mode = AutoPanMode.CompassNavigation;
        }

        public IconElement OffIcon
        {
            get { return (IconElement)GetValue(OffIconProperty); }
            set { SetValue(OffIconProperty, value); }
        }

        public static readonly DependencyProperty OffIconProperty =
                    DependencyProperty.Register("OffIcon", typeof(IconElement),
                    typeof(LocationDisplayToggle),
                    new PropertyMetadata(new SymbolIcon(Symbol.Target)
                    {
                        Foreground = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255))
                    }, OnIconPropertyChanged));


        public IconElement OnIcon
        {
            get { return (IconElement)GetValue(OnIconProperty); }
            set { SetValue(OnIconProperty, value); }
        }

        public static readonly DependencyProperty OnIconProperty =
                    DependencyProperty.Register("OnIcon", typeof(IconElement),
                    typeof(LocationDisplayToggle),
                    new PropertyMetadata(new SymbolIcon(Symbol.Target), OnIconPropertyChanged));


        private static void OnIconPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LocationDisplayToggle)d).UpdateIcon();
        }

        public IconElement AutoPanIcon
        {
            get { return (IconElement)GetValue(AutoPanIconProperty); }
            set { SetValue(AutoPanIconProperty, value); }
        }

        public static readonly DependencyProperty AutoPanIconProperty =
                    DependencyProperty.Register("AutoPanIcon", typeof(IconElement), typeof(LocationDisplayToggle),
                    new PropertyMetadata(new FontIcon() { Glyph = "", FontSize = 16 }, OnIconPropertyChanged));



        public IconElement CompassIcon
        {
            get { return (IconElement)GetValue(CompassIconProperty); }
            set { SetValue(CompassIconProperty, value); }
        }

        public static readonly DependencyProperty CompassIconProperty =
            DependencyProperty.Register("Compass", typeof(IconElement),
            typeof(LocationDisplayToggle),
            new PropertyMetadata(new SymbolIcon(Symbol.View), OnIconPropertyChanged));


        private void UpdateIcon()
        {
            if (LocationDisplay == null)
                this.IsEnabled = false; //Disable button when no location display to control is present
            else
            {
                this.IsEnabled = true;
                if (!IsLocationEnabled)
                    Icon = OffIcon;
                else if (Mode == AutoPanMode.Off)
                    Icon = OnIcon;
                else if (Mode == AutoPanMode.Default)
                    Icon = AutoPanIcon;
                else if (Mode == AutoPanMode.CompassNavigation)
                    Icon = CompassIcon;
            }
            OffItem.IsChecked = !IsLocationEnabled;
            OnItem.IsChecked = Mode == AutoPanMode.Off && IsLocationEnabled;
            AutoPanItem.IsChecked = Mode == AutoPanMode.Default && IsLocationEnabled;
            CompassItem.IsChecked = Mode == AutoPanMode.CompassNavigation && IsLocationEnabled;
        }

        public event EventHandler<AutoPanModeChangedEventArgs> AutoPanModeChanged;
        private void OnAutoPanModeChanged(AutoPanMode oldMode, AutoPanMode newMode)
        {
            if (AutoPanModeChanged != null)
                AutoPanModeChanged(this, new AutoPanModeChangedEventArgs(oldMode, newMode));
        }
    }

    public class AutoPanModeChangedEventArgs : EventArgs
    {
        public AutoPanMode OldMode { get; private set; }
        public AutoPanMode NewMode { get; private set; }
        internal AutoPanModeChangedEventArgs(AutoPanMode oldMode, AutoPanMode newMode)
        {
            OldMode = oldMode;
            NewMode = newMode;
        }
    }
}
