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
using Microsoft.Phone.Shell;
using WinMilk.Gui.Controls;
using System.Collections.ObjectModel;

namespace WinMilk.Gui
{
    public partial class TaskListPage : PhoneApplicationPage
    {
        public static bool s_Reload = true;

        public TaskListPage()
        {
            InitializeComponent();

            this.IsTasksLoading = false;
            
            InitializePopupAnimationCallbacks();
        }

        #region TaskLoading

        #region IsTasksLoading

        /// <summary>
        /// IsLoading Dependency Property
        /// </summary>
        public static readonly DependencyProperty IsTasksLoadingProperty =
            DependencyProperty.Register("IsTasksLoading", typeof(bool), typeof(TaskListPage),
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

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (s_Reload)
            {
                LoadData();
            }

            base.OnNavigatedTo(e);
        }

        private void LoadData()
        {
            // empty the lists
            /*listIncomplete.ItemsSource = new List<string>();
            listToday.ItemsSource = new List<string>();
            listTomorrow.ItemsSource = new List<string>();
            listWeek.ItemsSource = new List<string>();*/

            if (App.RtmClient.HasAuthToken)
            {
                this.IsTasksLoading = true;

                //Lists.ItemsSource = new List<RTM.TaskList>() { new RTM.TaskList(1, "Inbox", false), new RTM.TaskList(2, "Personal", true) };


                App.RtmClient.GetLists((ObservableCollection<RTM.TaskList> list) =>
                {
                    Lists.ItemsSource = list;
                    /*
                    App.Rest.GetAllIncompleteTasks((List<RTM.Task> incompleteTasks) =>
                    {
                        this.IsTasksLoading = false;

                        incompleteTasks.Sort();
                        listIncomplete.list.ItemsSource = incompleteTasks;

                        // Due on or before today
                        App.Rest.GetTasksDueOnOrBefore(DateTime.Today, (List<RTM.Task> dueToday) =>
                        {
                            this.IsTasksLoading = false;

                            dueToday.Sort();
                            listToday.list.ItemsSource = dueToday;
                        });

                        // Due tomorrow
                        App.Rest.GetTasksDueOn(DateTime.Today.AddDays(1), (List<RTM.Task> dueTomorrow) =>
                        {
                            this.IsTasksLoading = false;

                            dueTomorrow.Sort();
                            listTomorrow.list.ItemsSource = dueTomorrow;
                        });

                        // Due this week
                        DateTime nextSunday = DateTime.Today;
                        while (nextSunday.DayOfWeek != DayOfWeek.Sunday)
                        {
                            nextSunday = nextSunday.AddDays(1);
                        }

                        App.Rest.GetTasksDueOnOrBefore(nextSunday, (List<RTM.Task> dueThisWeek) =>
                        {
                            this.IsTasksLoading = false;

                            dueThisWeek.Sort();
                            listWeek.list.ItemsSource = dueThisWeek;
                        });
                    }, s_Reload);*/
                }, s_Reload);
                
            }
            else
            {
                Login();
            }

            s_Reload = false;
        }

        public void Login()
        {
            MessageBoxResult login = MessageBox.Show("You must log in and authenticate before continuing.", "Authenticate", MessageBoxButton.OK);
            this.NavigationService.Navigate(new Uri("/Gui/AuthPage.xaml", UriKind.Relative));
        }

        #endregion

        #region Application bar actions

        private void logout_Click(object sender, EventArgs e)
        {
            MessageBoxResult logout = MessageBox.Show("Log out and erase your settings?", "Log out", MessageBoxButton.OKCancel);
            if (logout == MessageBoxResult.OK)
            {
                App.RtmClient.DeleteData();
                Login();
            }
        }

        private void sync_Click(object sender, EventArgs e)
        {
            s_Reload = true;
            LoadData();
        }

        private void about_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Gui/AboutPage.xaml", UriKind.Relative));
        }

        private void add_Click(object sender, EventArgs e)
        {
            OpenSmartAdd();
        }

        #endregion

        #region SmartAdd Popup

        private AnimationCompletedDelegate popupOpened = () => { }, popupClosed = () => { };

        private void AdvancedAddButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.popupClosed = () =>
            {
                this.NavigationService.Navigate(new Uri("/Gui/AddTaskPage.xaml", UriKind.Relative));
            };

            this.CloseSmartAdd();
        }

        private void CancelSmartAddButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.CloseSmartAdd();
        }

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
                this.Overlay.Visibility = Visibility.Collapsed;
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
            }
        }

        #endregion

        #region ContextMenuOpened

        private bool _contextMenuOpened = false;

        private void TaskContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            _contextMenuOpened = true;
        }

        private void TaskContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            _contextMenuOpened = false;
        }

        #endregion


        private void TaskSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox list = sender as ListBox;

            if (!_contextMenuOpened)
            {
                if (list.SelectedIndex == -1)
                {
                    return;
                }

                List<RTM.Task> tasks = list.ItemsSource as List<RTM.Task>;
                RTM.Task selectedTask = tasks[list.SelectedIndex];

                this.NavigationService.Navigate(new Uri("/Gui/TaskDetailsPage.xaml?id=" + selectedTask.Id, UriKind.Relative));
            }

            list.SelectedIndex = -1;
        }

        private void Overlay_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // TODO: Add event handler implementation here.
            if (SmartAddPopup.IsOpen) CloseSmartAdd();
        }

        private void SmartAddButton_Click(object sender, EventArgs e)
        {
            SubmitSmartAdd();
        }

        private void SmartAddBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SubmitSmartAdd();
            }
        }

        private void SubmitSmartAdd()
        {
            SmartAddBox.IsEnabled = false;
            IsTasksLoading = true;

            App.RtmClient.AddTaskWithSmartAdd(SmartAddBox.Text, () =>
            {
                CloseSmartAdd();
                LoadData();
                IsTasksLoading = false;
            });
        }

        private void settings_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Gui/SettingsPage.xaml", UriKind.Relative));
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            CreateApplicationBar();
        }

        private void CreateApplicationBar()
        {
            /*
            // Build ApplicationBar with localized strings
            ApplicationBar = new ApplicationBar();

            ApplicationBarIconButton add = new ApplicationBarIconButton(new Uri("/icons/appbar.add.rest.png", UriKind.Relative));
            add.Text = AppResources.AddTaskAppbar;
            add.Click += new EventHandler(add_Click);
            ApplicationBar.Buttons.Add(add);

            ApplicationBarIconButton sync = new ApplicationBarIconButton(new Uri("/icons/appbar.refresh.rest.png", UriKind.Relative));
            sync.Text = AppResources.SyncAppbar;
            sync.Click += new EventHandler(sync_Click);
            ApplicationBar.Buttons.Add(sync);

            ApplicationBarMenuItem settings = new ApplicationBarMenuItem(AppResources.SettingsAppbar);
            settings.Click += new EventHandler(settings_Click);
            ApplicationBar.MenuItems.Add(settings);

            ApplicationBarMenuItem logout = new ApplicationBarMenuItem(AppResources.LogoutAppbar);
            logout.Click += new EventHandler(logout_Click);
            ApplicationBar.MenuItems.Add(logout);

            ApplicationBarMenuItem about = new ApplicationBarMenuItem(AppResources.AboutAppbar);
            about.Click += new EventHandler(about_Click);
            ApplicationBar.MenuItems.Add(about);
             * */
        }

        private void Lists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox list = sender as ListBox;

            if (list.SelectedIndex == -1)
            {
                return;
            }

            List<RTM.TaskList> lists = list.ItemsSource as List<RTM.TaskList>;
            RTM.TaskList selected = lists[list.SelectedIndex];

            this.NavigationService.Navigate(new Uri("/Gui/ListPage.xaml?id=" + selected.Id, UriKind.Relative));

            list.SelectedIndex = -1;
        }
    }

    delegate void AnimationCompletedDelegate();
}