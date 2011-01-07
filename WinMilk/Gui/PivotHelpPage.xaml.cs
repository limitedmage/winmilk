using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;

namespace WinMilk
{
    public partial class PivotHelpPage : PhoneApplicationPage
    {
        public PivotHelpPage()
        {
            InitializeComponent();
        }

        #region AboutPageEvents

        private void Author_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask page = new WebBrowserTask();
            page.URL = "http://julianapena.com";
            page.Show();
        }

        private void Site_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask page = new WebBrowserTask();
            page.URL = "http://julianapena.com/projects/winmilk";
            page.Show();
        }

        private void License_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask page = new WebBrowserTask();
            page.URL = "http://winmilk.codeplex.com/license";
            page.Show();
        }

        private void Codeplex_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask page = new WebBrowserTask();
            page.URL = "http://winmilk.codeplex.com/";
            page.Show();
        }

        /*
        private void Coffee_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask page = new WebBrowserTask();
            page.URL = "http://julianapena.com/donate.html?ref=WinMilk";
            page.Show();
        }
        */

        private void RTM_Click(object senter, RoutedEventArgs e)
        {
            WebBrowserTask page = new WebBrowserTask();
            page.URL = "http://rememberthemilk.com";
            page.Show();
        }

        private void Report_Click(object sender, RoutedEventArgs e)
        {
            EmailComposeTask emailComposeTask = new EmailComposeTask();
            emailComposeTask.To = "winmilk@julianapena.com";
            emailComposeTask.Subject = "WinMilk bug or suggestion";
            emailComposeTask.Show();
        }

        private void Review_Click(object sender, RoutedEventArgs e)
        {
            MarketplaceReviewTask r = new MarketplaceReviewTask();
            r.Show();
        }

        #endregion 

        /// <summary>
        ///     Will select the pivot item that was specified in the Uri.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            const string page = "Page";
            IDictionary<string, string> parameters = this.NavigationContext.QueryString;
            if (parameters.ContainsKey(page))
            {
                string pageValue = parameters[page];
                if (string.Equals(pageValue, "About", StringComparison.OrdinalIgnoreCase))
                {
                    // lets navigate to the about page since it was selected in the URI
                    HelpPivot.SelectedItem = About;
                }
                else if (string.Equals(pageValue, "Settings", StringComparison.OrdinalIgnoreCase))
                {
                    // lets navigate to the about page since it was selected in the URI
                    HelpPivot.SelectedItem = Settings;
                }
            }
            base.OnNavigatedTo(e);
        }
    }
}
