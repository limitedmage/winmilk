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
	public partial class TaskListPage : PhoneApplicationPage {
		private RTM.RestClient rtm;

		public TaskListPage() {
			InitializeComponent();
		}

		private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e) {
			rtm = new RTM.RestClient(Helper.IsolatedStorageHelper.GetObject<string>("token"));

			rtm.GetTaskList((List<RTM.Task> list) => {
				list.Sort((RTM.Task a, RTM.Task b) => {
					return a.Due.CompareTo(b.Due);
				});
				listBox1.ItemsSource = list;
			});
		}
	}
}