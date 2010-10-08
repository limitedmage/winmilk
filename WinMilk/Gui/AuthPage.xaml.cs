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

namespace WinMilk.Gui {
	public partial class AuthPage : PhoneApplicationPage {
		private RTM.RestClient rtm;

		public AuthPage() {
			this.rtm = new RTM.RestClient();
			InitializeComponent();
		}

		private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e) {

			this.rtm.GetAuthUrl((string url) => {
				webBrowser1.Navigate(new Uri(url));
			});
		}

		private void AuthDoneButton_Click(object sender, RoutedEventArgs e) {
			this.rtm.GetToken((string token) => {
				Helper.IsolatedStorageHelper.SaveObject<string>("token", token);

                this.NavigationService.GoBack();
			});
		}
	}
}