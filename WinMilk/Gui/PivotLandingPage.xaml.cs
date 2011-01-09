using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using IronCow;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using WinMilk.Helper;
using WinMilk.Gui.Controls;

namespace WinMilk.Gui
{
    public partial class PivotLandingPage : PhoneApplicationPage
    {
        #region IsLoading Property

        public static bool sReload = true;

        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register("IsLoading", typeof(bool), typeof(PivotLandingPage),
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
               DependencyProperty.Register("TodayTasks", typeof(ObservableCollection<Task>), typeof(PivotLandingPage),
                   new PropertyMetadata(new ObservableCollection<Task>()));

        public ObservableCollection<Task> TomorrowTasks
        {
            get { return (ObservableCollection<Task>)GetValue(TomorrowTasksProperty); }
            set { SetValue(TomorrowTasksProperty, value); }
        }

        public static readonly DependencyProperty TomorrowTasksProperty =
               DependencyProperty.Register("TomorrowTasks", typeof(ObservableCollection<Task>), typeof(PivotLandingPage),
                   new PropertyMetadata(new ObservableCollection<Task>()));

        public ObservableCollection<Task> OverdueTasks
        {
            get { return (ObservableCollection<Task>)GetValue(OverdueTasksProperty); }
            set { SetValue(OverdueTasksProperty, value); }
        }

        public static readonly DependencyProperty OverdueTasksProperty =
               DependencyProperty.Register("OverdueTasks", typeof(ObservableCollection<Task>), typeof(PivotLandingPage),
                   new PropertyMetadata(new ObservableCollection<Task>()));

        public ObservableCollection<Task> NoDueTasks
        {
            get { return (ObservableCollection<Task>)GetValue(NoDueTasksProperty); }
            set { SetValue(NoDueTasksProperty, value); }
        }

        public static readonly DependencyProperty NoDueTasksProperty =
               DependencyProperty.Register("NoDueTasks", typeof(ObservableCollection<Task>), typeof(PivotLandingPage),
                   new PropertyMetadata(new ObservableCollection<Task>()));

        public ObservableCollection<Task> WeekTasks
        {
            get { return (ObservableCollection<Task>)GetValue(WeekTasksProperty); }
            set { SetValue(WeekTasksProperty, value); }
        }

        public static readonly DependencyProperty WeekTasksProperty =
               DependencyProperty.Register("WeekTasks", typeof(ObservableCollection<Task>), typeof(PivotLandingPage),
                   new PropertyMetadata(new ObservableCollection<Task>()));

        public ObservableCollection<TaskList> TaskLists
        {
            get { return (ObservableCollection<TaskList>)GetValue(TaskListsProperty); }
            set { SetValue(TaskListsProperty, value); }
        }

        public static readonly DependencyProperty TaskListsProperty =
               DependencyProperty.Register("TaskLists", typeof(ObservableCollection<TaskList>), typeof(PivotLandingPage),
                   new PropertyMetadata(new ObservableCollection<TaskList>()));

        public readonly static DependencyProperty TagsProperty =
            DependencyProperty.Register("Tags", typeof(SortableObservableCollection<string>), typeof(PivotLandingPage),
                new PropertyMetadata((SortableObservableCollection<string>)null));

        public SortableObservableCollection<string> Tags
        {
            get { return (SortableObservableCollection<string>)GetValue(TagsProperty); }
            set { SetValue(TagsProperty, value); }
        }

        #endregion

        #region Construction, Loading and Navigating

        public PivotLandingPage()
        {
            InitializeComponent();
            CreateApplicationBar();

            // Setup our context menus.
            OverdueList.AddContextMenu(TaskListContextMenuClick);
            TodayList.AddContextMenu(TaskListContextMenuClick);
            TomorrowList.AddContextMenu(TaskListContextMenuClick);
            WeekList.AddContextMenu(TaskListContextMenuClick);
            NoDueList.AddContextMenu(TaskListContextMenuClick);

            IsLoading = false;

            // Start on the pivot that the user has selected on the settings page.
            AppSettings settings = new AppSettings();
            if (settings.StartPageSetting == 0)
            {
                Pivot.SelectedItem = TasksPanel;
            }
            else if (settings.StartPageSetting == 1)
            {
                Pivot.SelectedItem = ListsPanel;
            }
            else if (settings.StartPageSetting == 2)
            {
                Pivot.SelectedItem = TagsPanel;
            }
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
            SyncData();
        }

        private void CreateApplicationBar()
        {
            // Build ApplicationBar with localized strings
            ApplicationBar = new ApplicationBar();

            ApplicationBarIconButton add = new ApplicationBarIconButton(new Uri("/icons/appbar.add.rest.png", UriKind.Relative));
            add.Text = AppResources.AddTaskAppbar;
            add.Click += new EventHandler(AddTask_Click);
            ApplicationBar.Buttons.Add(add);

            ApplicationBarIconButton sync = new ApplicationBarIconButton(new Uri("/icons/appbar.refresh.rest.png", UriKind.Relative));
            sync.Text = AppResources.SyncAppbar;
            sync.Click += new EventHandler(Sync_Click);
            ApplicationBar.Buttons.Add(sync);

            ApplicationBarIconButton search = new ApplicationBarIconButton(new Uri("/icons/appbar.feature.search.rest.png", UriKind.Relative));
            search.Text = AppResources.MoreSearchButton;
            search.Click += new EventHandler(SearchButton_Click);
            ApplicationBar.Buttons.Add(search);

            ApplicationBarMenuItem logout = new ApplicationBarMenuItem(AppResources.MoreLogoutButton);
            logout.Click += new EventHandler(LogoutButton_Click);
            ApplicationBar.MenuItems.Add(logout);

            ApplicationBarMenuItem about = new ApplicationBarMenuItem(AppResources.MoreAboutButton);
            about.Click += new EventHandler(AboutButton_Click);
            ApplicationBar.MenuItems.Add(about);

            ApplicationBarMenuItem settings = new ApplicationBarMenuItem(AppResources.MoreSettingsButton);
            settings.Click += new EventHandler(SettingsButton_Click);
            ApplicationBar.MenuItems.Add(settings);

            /*** Removed as per Microsoft Policies :( ***/
            /*ApplicationBarMenuItem donate = new ApplicationBarMenuItem(AppResources.MoreDonateButton);
            donate.Click += new EventHandler(DonateButton_Click);
            ApplicationBar.MenuItems.Add(donate);*/

            ApplicationBarMenuItem report = new ApplicationBarMenuItem(AppResources.MoreReportButton);
            report.Click += new EventHandler(ReportButton_Click);
            ApplicationBar.MenuItems.Add(report);
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
            BackgroundWorker b = new BackgroundWorker();
            b.DoWork += (s, e) =>
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
            };

            b.RunWorkerAsync();
        }

        private void LoadData()
        {
            BackgroundWorker b = new BackgroundWorker();
            b.DoWork += (s, e) =>
            {
                LoadDataInBackground();
            };
            b.RunWorkerAsync();
        }

        private void LoadDataInBackground()
        {
            if (App.RtmClient.TaskLists != null)
            {
                var tempTaskLists = new SortableObservableCollection<TaskList>();

                var tempOverdueTasks = new SortableObservableCollection<Task>();
                var tempTodayTasks = new SortableObservableCollection<Task>();
                var tempTomorrowTasks = new SortableObservableCollection<Task>();
                var tempWeekTasks = new SortableObservableCollection<Task>();
                var tempNoDueTasks = new SortableObservableCollection<Task>();

                var tempTags = new SortableObservableCollection<string>();

                foreach (TaskList l in App.RtmClient.TaskLists)
                {
                    tempTaskLists.Add(l);

                    if (l.IsNormal && l.Tasks != null)
                    {
                        foreach (Task task in l.Tasks)
                        {
                            // add tags
                            foreach (string tag in task.Tags)
                            {
                                if (!tempTags.Contains(tag)) tempTags.Add(tag);
                            }

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

                tempOverdueTasks.Sort();
                tempTodayTasks.Sort();
                tempTomorrowTasks.Sort();
                tempWeekTasks.Sort();
                tempNoDueTasks.Sort();

                tempTags.Sort();

                SmartDispatcher.BeginInvoke(() =>
                {
                    TaskLists = tempTaskLists;

                    TodayTasks = tempTodayTasks;
                    TomorrowTasks = tempTomorrowTasks;
                    OverdueTasks = tempOverdueTasks;
                    WeekTasks = tempWeekTasks;
                    NoDueTasks = tempNoDueTasks;

                    Tags = tempTags;
                });
            }
        }

        public void Login()
        {
            SmartDispatcher.BeginInvoke(() =>
            {
                MessageBoxResult login = MessageBox.Show(AppResources.LogInMessageBoxText, AppResources.LogInMessageBoxTitle, MessageBoxButton.OK);
                this.NavigationService.Navigate(new Uri("/Gui/AuthPage.xaml", UriKind.Relative));
            });
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

        private void TagsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                string currentTag = e.AddedItems[0] as string;
                NavigationService.Navigate(new Uri("/Gui/TagPage.xaml?tag=" + Uri.EscapeUriString(currentTag), UriKind.Relative));
            }
        }

        private void AddTask_Click(object sender, EventArgs e)
        {
            AddTaskPopup.Open();
        }

        private void Sync_Click(object sender, EventArgs e)
        {
            sReload = true;
            SyncData();
        }

        private void LogoutButton_Click(object sender, EventArgs e)
        {
            MessageBoxResult logout = MessageBox.Show(AppResources.LogOutMessageBoxText, AppResources.LogOutMessageBoxTitle, MessageBoxButton.OKCancel);
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
            this.NavigationService.Navigate(new Uri("/Gui/PivotHelpPage.xaml?Page=About", UriKind.Relative));
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Gui/PivotHelpPage.xaml?Page=Settings", UriKind.Relative));
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

        private void ReportButton_Click(object sender, EventArgs e)
        {
            EmailComposeTask emailComposeTask = new EmailComposeTask();
            emailComposeTask.To = "winmilk@julianapena.com";
            emailComposeTask.Subject = "WinMilk bug or suggestion";
            emailComposeTask.Show();
        }

        /// <summary>
        ///     This event is fired when a context menu action for an item is selected (Complete/Postpone).
        /// </summary>
        private void TaskListContextMenuClick(string menuItem, Task task)
        {
            // now that we have the associated task, we can take action on it.
            if (menuItem == "Complete")
            {
                task.Complete(() =>
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        IsLoading = false;
                    });

                    sReload = true;
                    LoadData();
                });
            }
            else if (menuItem == "Postpone")
            {
                task.Postpone(() =>
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        IsLoading = false;
                    });

                    sReload = true;
                    LoadData();
                });
            }
        }

        /*** Removed as per Microsoft Policies :( ***/
        /*
        private void DonateButton_Click(object sender, EventArgs e)
        {
            WebBrowserTask page = new WebBrowserTask();
            page.URL = "http://julianapena.com/donate.html?ref=WinMilk";
            page.Show();
        }
        */

        #endregion
    }
}
