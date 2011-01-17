using System;
using System.Windows;
using IronCow;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Info;
using System.Windows.Controls;

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

        private string Frob { get; set; }

        public AuthPage()
        {
            InitializeComponent();
        }

        private void StartAuth()
        {
            // track authentication attempt
            var an = new Helper.AnalyticsHelper();
            var value = (byte[])DeviceExtendedProperties.GetValue("DeviceUniqueId");
            var id = Convert.ToBase64String(value);
            an.Track("AuthAttempt", id);

            this.IsLoading = true;

            App.RtmClient.GetFrob((string frob) =>
            {
                Frob = frob;
                string url = App.RtmClient.GetAuthenticationUrl(frob, AuthenticationPermissions.Delete);

                Dispatcher.BeginInvoke(() =>
                {
                    this.IsLoading = false;
                    webBrowser1.Navigate(new Uri(url));
                });
            });
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            //StartAuth();
            ShowAskAccount();
        }

        private void ShowAskAccount()
        {
            //ContentGrid.Children.Clear();
            
        }

        private void AuthDoneButton_Click(object sender, EventArgs e)
        {
            // only do something if Frob is present

            if (!string.IsNullOrEmpty(Frob))
            {
                IsLoading = true;
                App.RtmClient.GetToken(Frob, (string token, User user) =>
                {
                    // create timeline
                    App.RtmClient.GetOrStartTimeline((int timeline) =>
                    {
                        Dispatcher.BeginInvoke(() =>
                        {
                            IsLoading = false;

                            // track authentication success
                            var an = new Helper.AnalyticsHelper();
                            var value = (byte[])DeviceExtendedProperties.GetValue("DeviceUniqueId");
                            var id = Convert.ToBase64String(value);
                            an.Track("AuthSuccess", id);

                            if (NavigationService.CanGoBack)
                            {
                                PivotLandingPage.sReload = true;
                                NavigationService.GoBack();
                            }
                        });
                    });
                });
            }
        }

        private void RetryButton_Click(object sender, EventArgs e)
        {
            StartAuth();
        }

        private void webBrowser1_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            this.IsLoading = false;
        }

        private void webBrowser1_Navigating(object sender, NavigatingEventArgs e)
        {
            this.IsLoading = true;
        }

        private void NoAccountButton_Click(object sender, RoutedEventArgs e)
        {
            AskAccount.Visibility = Visibility.Collapsed;
            CreateAccountBrowser.Visibility = Visibility.Visible;
            AccountBrowser.Source = new Uri("http://www.rememberthemilk.com/signup/");
        }

        private void YesAccountButton_Click(object sender, RoutedEventArgs e)
        {
            AskAccount.Visibility = Visibility.Collapsed;
            AuthBrowser.Visibility = Visibility.Visible;
            StartAuth();
        }

        private void CreateDoneButton_Click(object sender, EventArgs e)
        {
            CreateAccountBrowser.Visibility = Visibility.Collapsed;
            AuthBrowser.Visibility = Visibility.Visible;
            StartAuth();
        }

        private void CreateRetryButton_Click(object sender, EventArgs e)
        {
            AccountBrowser.Source = new Uri("http://www.rememberthemilk.com/signup/");
        }
    }
}