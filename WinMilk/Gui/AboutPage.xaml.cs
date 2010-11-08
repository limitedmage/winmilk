using System;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;

namespace WinMilk.Gui
{
    public partial class AboutPage : PhoneApplicationPage
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, EventArgs e)
        {
            this.NavigationService.GoBack();
        }

        private void Author_Click(object sender, RoutedEventArgs e)
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

        private void Coffee_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask page = new WebBrowserTask();
            page.URL = "http://julianapena.com/donate.html?ref=WinMilk";
            page.Show();
        }

        private void RTM_Click(object senter, RoutedEventArgs e)
        {
            WebBrowserTask page = new WebBrowserTask();
            page.URL = "http://rememberthemilk.com";
            page.Show();
        }
    }
}