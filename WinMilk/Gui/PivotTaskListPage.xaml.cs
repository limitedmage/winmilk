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

namespace WinMilk.Gui
{
    public partial class PivotTaskListPage : PhoneApplicationPage
    {
        private RTM.RestClient rtm;

        #region IsTasksLoading

        /// <summary>
        /// IsLoading Dependency Property
        /// </summary>
        public static readonly DependencyProperty IsTasksLoadingProperty =
            DependencyProperty.Register("IsTasksLoading", typeof(bool), typeof(PivotTaskListPage),
                new PropertyMetadata((bool)false));

        /// <summary>
        /// Gets or sets the IsTwitsLoading property. This dependency property 
        /// indicates whether we are currently loading twits.
        /// </summary>
        public bool IsTasksLoading
        {
            get { return (bool)GetValue(IsTasksLoadingProperty); }
            set { SetValue(IsTasksLoadingProperty, value); }
        }

        #endregion

        public PivotTaskListPage()
        {
            InitializeComponent();

            this.IsTasksLoading = false;
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            LoadData();

            base.OnNavigatedTo(e);
        }

        private void LoadData()
        {
            listIncomplete.ItemsSource = new List<string>(); // empty the list

            if (Helper.IsolatedStorageHelper.Contains("token"))
            {
                rtm = new RTM.RestClient(Helper.IsolatedStorageHelper.GetObject<string>("token"));

                this.IsTasksLoading = true;
                rtm.GetTaskList((List<RTM.Task> list) =>
                {
                    this.IsTasksLoading = false;

                    list.Sort((RTM.Task a, RTM.Task b) =>
                    {
                        return a.Due.CompareTo(b.Due);
                    });
                    listIncomplete.ItemsSource = list;
                });
            }
            else
            {
                MessageBoxResult login = MessageBox.Show("You must log in and authenticate before continuing.", "Authenticate", MessageBoxButton.OK);
                this.NavigationService.Navigate(new Uri("/Gui/AuthPage.xaml", UriKind.Relative));
            }
        }

        private void logout_Click(object sender, EventArgs e)
        {
            MessageBoxResult logout = MessageBox.Show("Log out and erase your settings?", "Log out", MessageBoxButton.OKCancel);
            if (logout == MessageBoxResult.OK)
            {
                Helper.IsolatedStorageHelper.DeleteObject("token");
                this.LoadData();
            }
        }

        private void sync_Click(object sender, EventArgs e)
        {
            this.LoadData();
        }

        private void add_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Gui/AddTaskPage.xaml", UriKind.Relative));
        }
    }
}