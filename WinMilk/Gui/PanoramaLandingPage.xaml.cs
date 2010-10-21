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
using IronCow.Rest;

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
               DependencyProperty.Register("TodayTasks", typeof(ObservableCollection<RawTask>), typeof(PanoramaLandingPage),
                   new PropertyMetadata(new ObservableCollection<RawTask>()));

        public ObservableCollection<RawTask> TodayTasks
        {
            get { return (ObservableCollection<RawTask>)GetValue(TodayTasksProperty); }
            set { SetValue(TodayTasksProperty, value); }
        }

        public ObservableCollection<RawTask> TomorrowTasks
        {
            get { return (ObservableCollection<RawTask>)GetValue(TomorrowTasksProperty); }
            set { SetValue(TomorrowTasksProperty, value); }
        }

        public static readonly DependencyProperty TomorrowTasksProperty =
               DependencyProperty.Register("TomorrowTasks", typeof(ObservableCollection<RawTask>), typeof(PanoramaLandingPage),
                   new PropertyMetadata(new ObservableCollection<RawTask>()));

        public ObservableCollection<RawList> TaskLists
        {
            get { return (ObservableCollection<RawList>)GetValue(TaskListsProperty); }
            set { SetValue(TaskListsProperty, value); }
        }

        public static readonly DependencyProperty TaskListsProperty =
               DependencyProperty.Register("TaskLists", typeof(ObservableCollection<RawList>), typeof(PanoramaLandingPage),
                   new PropertyMetadata(new ObservableCollection<RawList>()));


        public PanoramaLandingPage()
        {
            InitializeComponent();

            IsLoading = false;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            LoadData();

            base.OnNavigatedTo(e);
        }

        private void LoadData()
        {
            if (App.RtmClient.AuthToken != null)
            {
                this.IsLoading = true;

                App.RtmClient.GetLists((RawList[] lists) =>
                {
                    foreach (RawList l in lists)
                    {
                        TaskLists.Add(l);
                    }

                    App.RtmClient.GetTasks(null, "", null, (RawList[] listsWithTasks) => 
                    { 
                        foreach (RawList l in listsWithTasks)
                        {
                            foreach (RawTaskSeries s in l.TaskSeries)
                            {
                                foreach (RawTask t in s.Tasks)
                                {
                                    
                                }
                            }

                        }
                    });
                });


                App.RtmClient.GetLists((ObservableCollection<RTM.TaskList> lists) =>
                {
                    TaskLists = lists;

                    App.RtmClient.GetAllIncompleteTasks((ObservableCollection<RTM.Task> incompleteTasks) =>
                    {
                        this.IsLoading = false;

                        // Due on or before today
                        App.RtmClient.GetTasks("dueBefore:tomorrow", (Task[] tasks) => 
                        {
                            foreach (Task t in tasks)
                            {
                                TodayTasks.Add(t);
                            }
                        });

                        // Due tomorrow
                        App.RtmClient.GetTasks("due:tomorrow", (Task[] tasks) =>
                        {
                            foreach (Task t in tasks)
                            {
                                TomorrowTasks.Add(t);
                            }
                        });

                        /*
                        App.Rest.GetTasksInListsInOrder(lists, 0, (loadedLists) => 
                        {
                            this.IsLoading = false;
                        });*/

                        // get count for tasks in non-smart lists
                        foreach (RTM.TaskList l in lists)
                        {
                            if (l.IsNormal)
                            {
                                App.RtmClient.GetTasksInList(l, (t) => { }, false);
                            }
                        }

                    }, _reload, RTM.Task.CompareByDate);
                }, _reload);

            }
            else
            {
                Login();
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

            this.NavigationService.Navigate(new Uri("/Gui/ListPage.xaml?id=" + selected.Id, UriKind.Relative));

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

            App.RtmClient.AddTaskWithSmartAdd(SmartAddBox.Text, () =>
            {
                IsLoading = false;
                CloseSmartAdd();
                LoadData();
            });
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
