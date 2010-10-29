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
using Microsoft.Phone.Tasks;
using WinMilk.Helper;

namespace WinMilk.Gui
{
    public partial class PanoramaLandingPage : PhoneApplicationPage
    {
        #region IsLoading Property

        public static bool sReload = true;

        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register("IsLoading", typeof(bool), typeof(PanoramaLandingPage),
                new PropertyMetadata((bool)false));

        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        #endregion

        #region Task Lists Properties

        public ObservableCollection<Task> TodayTasks
        {
            get { return (ObservableCollection<Task>)GetValue(TodayTasksProperty); }
            set { SetValue(TodayTasksProperty, value); }
        }

        public static readonly DependencyProperty TodayTasksProperty =
               DependencyProperty.Register("TodayTasks", typeof(ObservableCollection<Task>), typeof(PanoramaLandingPage),
                   new PropertyMetadata(new ObservableCollection<Task>()));

        public ObservableCollection<Task> TomorrowTasks
        {
            get { return (ObservableCollection<Task>)GetValue(TomorrowTasksProperty); }
            set { SetValue(TomorrowTasksProperty, value); }
        }

        public static readonly DependencyProperty TomorrowTasksProperty =
               DependencyProperty.Register("TomorrowTasks", typeof(ObservableCollection<Task>), typeof(PanoramaLandingPage),
                   new PropertyMetadata(new ObservableCollection<Task>()));

        public ObservableCollection<Task> OverdueTasks
        {
            get { return (ObservableCollection<Task>)GetValue(OverdueTasksProperty); }
            set { SetValue(OverdueTasksProperty, value); }
        }

        public static readonly DependencyProperty OverdueTasksProperty =
               DependencyProperty.Register("OverdueTasks", typeof(ObservableCollection<Task>), typeof(PanoramaLandingPage),
                   new PropertyMetadata(new ObservableCollection<Task>()));

        public ObservableCollection<Task> NoDueTasks
        {
            get { return (ObservableCollection<Task>)GetValue(NoDueTasksProperty); }
            set { SetValue(NoDueTasksProperty, value); }
        }

        public static readonly DependencyProperty NoDueTasksProperty =
               DependencyProperty.Register("NoDueTasks", typeof(ObservableCollection<Task>), typeof(PanoramaLandingPage),
                   new PropertyMetadata(new ObservableCollection<Task>()));

        public ObservableCollection<Task> WeekTasks
        {
            get { return (ObservableCollection<Task>)GetValue(WeekTasksProperty); }
            set { SetValue(WeekTasksProperty, value); }
        }

        public static readonly DependencyProperty WeekTasksProperty =
               DependencyProperty.Register("WeekTasks", typeof(ObservableCollection<Task>), typeof(PanoramaLandingPage),
                   new PropertyMetadata(new ObservableCollection<Task>()));

        public ObservableCollection<TaskList> TaskLists
        {
            get { return (ObservableCollection<TaskList>)GetValue(TaskListsProperty); }
            set { SetValue(TaskListsProperty, value); }
        }

        public static readonly DependencyProperty TaskListsProperty =
               DependencyProperty.Register("TaskLists", typeof(ObservableCollection<TaskList>), typeof(PanoramaLandingPage),
                   new PropertyMetadata(new ObservableCollection<TaskList>()));

        #endregion

        #region Construction, Loading and Navigating

        public PanoramaLandingPage()
        {
            InitializeComponent();

            IsLoading = false;
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {

        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            LoadData();
            SyncData();

            base.OnNavigatedTo(e);
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (AddTaskPopup.IsOpen)
            {
                AddTaskPopup.Close();
                e.Cancel = true;
            }

            base.OnBackKeyPress(e);
        }

        #endregion

        #region Loading Data

        private void SyncData()
        {
            if (sReload)
            {
                if (!string.IsNullOrEmpty(App.RtmClient.AuthToken))
                {
                    SmartDispatcher.BeginInvoke(() =>
                    {
                        this.IsLoading = true;
                    });

                    App.RtmClient.SyncEverything(() =>
                    {
                        LoadData();

                        SmartDispatcher.BeginInvoke(() =>
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

            sReload = false;
        }

        private void LoadData()
        {
            if (App.RtmClient.TaskLists != null)
            {
                var tempTaskLists = new ObservableCollection<TaskList>();

                var tempOverdueTasks = new ObservableCollection<Task>();
                var tempTodayTasks = new ObservableCollection<Task>();
                var tempTomorrowTasks = new ObservableCollection<Task>();
                var tempWeekTasks = new ObservableCollection<Task>();
                var tempNoDueTasks = new ObservableCollection<Task>();

                foreach (TaskList l in App.RtmClient.TaskLists)
                {
                    tempTaskLists.Add(l);

                    if (l.IsNormal && l.Tasks != null)
                    {
                        foreach (Task task in l.Tasks)
                        {
                            if (task.IsIncomplete)
                            {
                                if (task.DueDateTime.HasValue)
                                {
                                    // overdue
                                    if (task.DueDateTime.Value < DateTime.Today || (task.HasDueTime && task.DueDateTime.Value < DateTime.Now))
                                    {
                                        tempOverdueTasks.Add(task);
                                    }
                                    // today
                                    else if (task.DueDateTime.Value.Date == DateTime.Today)
                                    {
                                        tempTodayTasks.Add(task);
                                    }
                                    // tomorrow
                                    else if (task.DueDateTime.Value.Date == DateTime.Today.AddDays(1))
                                    {
                                        tempTomorrowTasks.Add(task);
                                    }
                                    // this week
                                    else if (task.DueDateTime.Value.Date > DateTime.Today.AddDays(1) && task.DueDateTime.Value.Date <= DateTime.Today.AddDays(6))
                                    {
                                        tempWeekTasks.Add(task);
                                    }
                                }
                                else
                                {
                                    // no due
                                    tempNoDueTasks.Add(task);
                                }
                            }
                        }
                    }
                }

                SmartDispatcher.BeginInvoke(() =>
                {
                    TaskLists = tempTaskLists;
                    TodayTasks = tempTodayTasks;
                    TomorrowTasks = tempTomorrowTasks;
                    OverdueTasks = tempOverdueTasks;
                    WeekTasks = tempWeekTasks;
                    NoDueTasks = tempNoDueTasks;
                });
            }
        }

        public void Login()
        {
            MessageBoxResult login = MessageBox.Show("You must log in and authenticate before continuing.", "Authenticate", MessageBoxButton.OK);
            this.NavigationService.Navigate(new Uri("/Gui/AuthPage.xaml", UriKind.Relative));
        }

        #endregion

        #region Event Handling

        private void ListsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox list = sender as ListBox;

            if (list.SelectedIndex == -1)
            {
                return;
            }

            ObservableCollection<TaskList> lists = list.ItemsSource as ObservableCollection<TaskList>;
            TaskList selected = lists[list.SelectedIndex];

            this.NavigationService.Navigate(new Uri("/Gui/ListPage.xaml?id=" + selected.Id, UriKind.Relative));

            list.SelectedIndex = -1;
        }

        private void AddTask_Click(object sender, EventArgs e)
        {
            AddTaskPopup.Open();
        }

        private void Sync_Click(object sender, EventArgs e)
        {
            sReload = true;
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
            this.NavigationService.Navigate(new Uri("/Gui/SearchPage.xaml", UriKind.Relative));
        }

        private void TagsButton_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Gui/TagListPage.xaml", UriKind.Relative));
        }

        private void AboutButton_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Gui/AboutPage.xaml", UriKind.Relative));
        }

        private void AddTaskPopup_Submit(object sender, WinMilk.Gui.Controls.SubmitEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                IsLoading = true;
            });
            App.RtmClient.AddTask(e.Text, true, null, () =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    IsLoading = false;
                });

                sReload = true;
                LoadData();
            });
        }

        private void DonateButton_Click(object sender, EventArgs e)
        {
            WebBrowserTask page = new WebBrowserTask();
            page.URL = "http://julianapena.com/donate.html?ref=WinMilk";
            page.Show();
        }

        #endregion
    }
}
