using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using WinMilk.Gui.Controls.ProgressBar;

namespace WinMilk.Gui
{
    public partial class AuthPage : PhoneApplicationPage
    {

        #region IsLoading

        /// <summary>
        /// IsLoading Dependency Property
        /// </summary>
        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register("IsLoading", typeof(bool), typeof(AuthPage),
                new PropertyMetadata((bool)false));

        /// <summary>
        /// Gets or sets the IsLoading property. This dependency property 
        /// indicates whether we are currently loading.
        /// </summary>
        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        #endregion

        public AuthPage()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {

            this.IsLoading = true;

            App.Rest.GetAuthUrl((string url) =>
            {
                this.IsLoading = false;
                webBrowser1.Navigate(new Uri(url));
            });
        }

        private void AuthDoneButton_Click(object sender, EventArgs e)
        {
            this.IsLoading = true;
            App.Rest.GetToken((string token) =>
            {
                this.IsLoading = false;

                TaskListPage.s_Reload = true;

                this.NavigationService.GoBack();
            });
        }

        private void webBrowser1_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            this.IsLoading = false;
        }

        private void webBrowser1_Navigating(object sender, NavigatingEventArgs e)
        {
            this.IsLoading = true;
        }
    }
}