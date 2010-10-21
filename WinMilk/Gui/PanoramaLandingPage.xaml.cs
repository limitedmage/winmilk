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
using System.Windows.Data;
using System.Collections.ObjectModel;
using IronCow;

namespace WinMilk
{
    public partial class PanoramaLandingPage : PhoneApplicationPage
    {
        public bool _reload = true;

        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register("IsLoading", typeof(bool), typeof(PanoramaLandingPage),
                new PropertyMetadata((bool)false));

        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        public static readonly DependencyProperty TodayTasksProperty =
               DependencyProperty.Register("TodayTasks", typeof(ObservableCollection<Task>), typeof(PanoramaLandingPage),
                   new PropertyMetadata(new ObservableCollection<Task>()));

        public ObservableCollection<Task> TodayTasks
        {
            get { return (ObservableCollection<Task>)GetValue(TodayTasksProperty); }
            set { SetValue(TodayTasksProperty, value); }
        }

        public ObservableCollection<Task> TomorrowTasks
        {
            get { return (ObservableCollection<Task>)GetValue(TomorrowTasksProperty); }
            set { SetValue(TomorrowTasksProperty, value); }
        }

        public static readonly DependencyProperty TomorrowTasksProperty =
               DependencyProperty.Register("TomorrowTasks", typeof(ObservableCollection<Task>), typeof(PanoramaLandingPage),
                   new PropertyMetadata(new ObservableCollection<Task>()));

        public ObservableCollection<TaskList> TaskLists
        {
            get { return (ObservableCollection<TaskList>)GetValue(TaskListsProperty); }
            set { SetValue(TaskListsProperty, value); }
        }

        public static readonly DependencyProperty TaskListsProperty =
               DependencyProperty.Register("TaskLists", typeof(ObservableCollection<TaskList>), typeof(PanoramaLandingPage),
                   new PropertyMetadata(new ObservableCollection<TaskList>()));


        public PanoramaLandingPage()
        {
            InitializeComponent();

            IsLoading = false;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            _reload = true;
            LoadData();

            base.OnNavigatedTo(e);
        }

        private void LoadData()
        {
            if (_reload)
            {
                if (!string.IsNullOrEmpty(App.RtmClient.AuthToken))
                {
                    this.IsLoading = true;

                    App.RtmClient.SyncEverything(() =>
                    {
                        Dispatcher.BeginInvoke(() =>
                        {
                            TaskLists = new ObservableCollection<TaskList>();
                            foreach (TaskList l in App.RtmClient.TaskLists)
                            {
                                TaskLists.Add(l);
                            }
                        });


                        Dispatcher.BeginInvoke(() =>
                        {
                            List<Task> today = App.RtmClient.GetTodayTasks();
                            TodayTasks = new ObservableCollection<Task>();
                            foreach (Task t in today)
                            {
                                TodayTasks.Add(t);
                            }

                            List<Task> tomorrow = App.RtmClient.GetTomorrowTasks();
                            TomorrowTasks = new ObservableCollection<Task>();
                            foreach (Task t in tomorrow)
                            {
                                TomorrowTasks.Add(t);
                            }
                        });

                        Dispatcher.BeginInvoke(() =>
                        {
                            IsLoading = false;
                        });
                    });
                }
                else
                {
                    Login();
                }
            }

            _reload = false;
        }

        public void Login()
        {
            MessageBoxResult login = MessageBox.Show("You must log in and authenticate before continuing.", "Authenticate", MessageBoxButton.OK);
            this.NavigationService.Navigate(new Uri("/Gui/AuthPage.xaml", UriKind.Relative));
        }

        private void ListsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox list = sender as ListBox;

            if (list.SelectedIndex == -1)
            {
                return;
            }

            ObservableCollection<TaskList> lists = list.ItemsSource as ObservableCollection<TaskList>;
            TaskList selected = lists[list.SelectedIndex];

            this.NavigationService.Navigate(new Uri("/Gui/PivotListPage.xaml?id=" + selected.Id, UriKind.Relative));

            list.SelectedIndex = -1;
        }

        delegate void AnimationCompletedDelegate();
        private AnimationCompletedDelegate popupOpened = () => { }, popupClosed = () => { };

        private void InitializePopupAnimationCallbacks()
        {
            this.OpenPopup.Completed += (object sender, EventArgs e) =>
            {
                popupOpened();
                popupOpened = () => { };
                this.SmartAddBox.Focus();
            };

            this.ClosePopup.Completed += (object sender, EventArgs e) =>
            {
                popupClosed();
                popupClosed = () => { };

                this.SmartAddPopup.IsOpen = false;
                this.SmartAddBox.Text = "";
                this.SmartAddBox.IsEnabled = true;
            };
        }

        private void OpenSmartAdd()
        {
            if (!this.SmartAddPopup.IsOpen)
            {
                this.SmartAddPopup.IsOpen = true;
                this.Overlay.Visibility = Visibility.Visible;

                this.OpenPopup.Begin();
            }
        }

        private void CloseSmartAdd()
        {
            if (this.SmartAddPopup.IsOpen)
            {
                this.ClosePopup.Begin();
                this.Overlay.Visibility = Visibility.Collapsed;
            }
        }

        private void SmartAddBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SubmitSmartAdd();
            }
        }

        private void SmartAddButton_Click(object sender, EventArgs e)
        {
            SubmitSmartAdd();
        }

        private void AdvancedAddButton_Click(object sender, RoutedEventArgs e)
        {
            this.popupClosed = () =>
            {
                this.NavigationService.Navigate(new Uri("/Gui/AddTaskPage.xaml", UriKind.Relative));
            };

            this.CloseSmartAdd();
        }

        private void CancelSmartAddButton_Click(object sender, RoutedEventArgs e)
        {
            CloseSmartAdd();
        }

        private void SubmitSmartAdd()
        {
            SmartAddBox.IsEnabled = false;
            IsLoading = true;

            /*
            App.RtmClient.AddTaskWithSmartAdd(SmartAddBox.Text, () =>
            {
                IsLoading = false;
                CloseSmartAdd();
                LoadData();
            });
             */
        }

        private void Overlay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SmartAddPopup.IsOpen) CloseSmartAdd();
        }

        private void AddTask_Click(object sender, EventArgs e)
        {
            OpenSmartAdd();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            InitializePopupAnimationCallbacks();
        }

        private void Sync_Click(object sender, EventArgs e)
        {
            _reload = true;
            LoadData();
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Gui/SettingsPage.xaml", UriKind.Relative));
        }

        private void LogoutButton_Click(object sender, EventArgs e)
        {
            MessageBoxResult logout = MessageBox.Show("Log out and erase your settings?", "Log out", MessageBoxButton.OKCancel);
            if (logout == MessageBoxResult.OK)
            {
                App.DeleteData();
                Login();
            }
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {

        }

        private void TagsButton_Click(object sender, EventArgs e)
        {

        }

        private void AboutButton_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Gui/AboutPage.xaml", UriKind.Relative));
        }
    }
}
