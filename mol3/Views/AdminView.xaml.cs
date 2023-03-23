using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Microsoft.VisualBasic;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace mol3.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AdminView : Page
    {
        private NavigationViewItem _lastItem;
        public AdminView()
        {
            this.InitializeComponent();
        }

        private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            //TODO: Error handling when page not found
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var item = args.InvokedItemContainer as NavigationViewItem;
            if (item == null || item == _lastItem)
                return;

            var clickedView = item.Tag?.ToString() ?? "SettingsView";
            if (!NavigateToView(clickedView)) return;
            _lastItem = item;
        }

        private bool NavigateToView(string clickedView)
        {
            var view = Assembly.GetExecutingAssembly().GetType($"mol3.Views.{clickedView}");

            if (string.IsNullOrWhiteSpace(clickedView) || view == null)
                return false;

            ContentFrame.Navigate(view, new EntranceNavigationTransitionInfo());
            return true;
        }
        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (ContentFrame.CanGoBack)
                ContentFrame.GoBack();
        }

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void CloseMenu_OnClick(object sender, RoutedEventArgs e)
        {
            if (CloseButton.IsEnabled)
            {
                Item.Visibility = Visibility.Collapsed;
                Item1.Visibility = Visibility.Collapsed;
                Item2.Visibility = Visibility.Collapsed;
                Item3.Visibility = Visibility.Collapsed;
                OpenButton.IsEnabled = false;
            }
        }

        private void OpenMenu_OnClick(object sender, RoutedEventArgs e)
        {
            if (OpenButton.IsEnabled)
            {
                Item.Visibility = Visibility.Visible;
                Item1.Visibility = Visibility.Visible;
                Item2.Visibility = Visibility.Visible;
                Item3.Visibility = Visibility.Visible;
                CloseButton.IsEnabled = false;
            }
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {

        }
    }
}
