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
using WinMilk.Gui.Controls;

namespace WinMilk.Gui
{
    public partial class TaskListPage : PhoneApplicationPage
    {
        public static bool s_Reload = false;

        private List<TaskListControl> TaskLists { get; set; }

        public TaskListPage()
        {
            InitializeComponent();

            this.IsTasksLoading = false;

            TaskLists = new List<TaskListControl>() {listIncomplete, listToday, listTomorrow, listWeek };
            foreach (TaskListControl control in TaskLists)
            {
                control.list.SelectionChanged += new SelectionChangedEventHandler(TaskSelectionChanged);
            }

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

            if (App.Rest.HasAuthToken)
            {
                this.IsTasksLoading = true;
                App.Rest.GetAllIncompleteTasks((List<RTM.Task> incompleteTasks) =>
                {
                    this.IsTasksLoading = false;

                    incompleteTasks.Sort((RTM.Task a, RTM.Task b) =>
                    {
                        return a.Due.CompareTo(b.Due);
                    });
                    listIncomplete.list.ItemsSource = incompleteTasks;

                    // Due on or before today
                    App.Rest.GetTasksDueOnOrBefore(DateTime.Today, (List<RTM.Task> dueToday) =>
                    {
                        this.IsTasksLoading = false;

                        dueToday.Sort((RTM.Task a, RTM.Task b) =>
                        {
                            return a.Due.CompareTo(b.Due);
                        });
                        listToday.list.ItemsSource = dueToday;
                    });

                    // Due tomorrow
                    App.Rest.GetTasksDueOn(DateTime.Today.AddDays(1), (List<RTM.Task> dueTomorrow) =>
                    {
                        this.IsTasksLoading = false;

                        dueTomorrow.Sort((RTM.Task a, RTM.Task b) =>
                        {
                            return a.Due.CompareTo(b.Due);
                        });
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

                        dueThisWeek.Sort((RTM.Task a, RTM.Task b) =>
                        {
                            return a.Due.CompareTo(b.Due);
                        });
                        listWeek.list.ItemsSource = dueThisWeek;
                    });
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
                App.Rest.DeleteData();
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
    }

    delegate void AnimationCompletedDelegate();
}