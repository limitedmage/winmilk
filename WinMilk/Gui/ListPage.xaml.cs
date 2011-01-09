using System;
using System.Windows;
using IronCow;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using WinMilk.Helper;

namespace WinMilk.Gui
{
    public partial class ListPage : PhoneApplicationPage
    {
        public static bool sReload = false;

        public static readonly DependencyProperty CurrentListProperty =
               DependencyProperty.Register("CurrentList", typeof(TaskList), typeof(ListPage),
                   new PropertyMetadata(new TaskList()));

        public TaskList CurrentList
        {
            get { return (TaskList)GetValue(CurrentListProperty); }
            set { SetValue(CurrentListProperty, value); }
        }

        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register("IsLoading", typeof(bool), typeof(ListPage),
                new PropertyMetadata((bool)false));

        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        public ListPage()
        {
            InitializeComponent();

            TaskList.AddContextMenu(TaskListContextMenuClick);
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            CreateApplicationBar();

            UpdateCurrentList();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (sReload)
            {
                ResyncLists();
                sReload = false;
            }

            base.OnNavigatedTo(e);
        }

        private void UpdateCurrentList()
        {
            string id;

            if (this.NavigationContext.QueryString.TryGetValue("id", out id))
            {
                // set current list
                CurrentList = App.RtmClient.TaskLists.GetById(id);

                // if current list is smart list, disable add button
                if (CurrentList.IsSmart)
                {
                    (ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = false;
                }
                else
                {
                    (ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = true;
                }
            }
        }

        private void ResyncLists()
        {
            SmartDispatcher.BeginInvoke(() =>
            {
                IsLoading = true;
            });

            App.RtmClient.CacheTasks(() =>
            {
                SmartDispatcher.BeginInvoke(() =>
                {
                    IsLoading = false;
                    var newList = App.RtmClient.TaskLists.GetById(CurrentList.Id);
                    CurrentList = null;
                    CurrentList = newList;
                });
            });
        }

        private void CreateApplicationBar()
        {
            // Build ApplicationBar with localized strings
            ApplicationBar = new ApplicationBar();

            ApplicationBarIconButton add = new ApplicationBarIconButton(new Uri("/icons/appbar.add.rest.png", UriKind.Relative));
            add.Text = AppResources.AddTaskAppbar;
            add.Click += new EventHandler(Add_Click);
            ApplicationBar.Buttons.Add(add);

            ApplicationBarIconButton sync = new ApplicationBarIconButton(new Uri("/icons/appbar.refresh.rest.png", UriKind.Relative));
            sync.Text = AppResources.SyncAppbar;
            sync.Click += new EventHandler(Sync_Click);
            ApplicationBar.Buttons.Add(sync);
        }

        private void Sync_Click(object sender, EventArgs e)
        {
            ResyncLists();
        }

        private void Add_Click(object sender, EventArgs e)
        {
            AddTask.Open();
        }

        private void AddTaskControl_Submit(object sender, Controls.SubmitEventArgs e)
        {
            SmartDispatcher.BeginInvoke(() =>
            {
                IsLoading = true;
            });

            string id = CurrentList.IsSmart ? null : CurrentList.Id;

            App.RtmClient.AddTask(e.Text, true, id, () =>
            {
                SmartDispatcher.BeginInvoke(() =>
                {
                    IsLoading = false;
                });

                ResyncLists();
            });
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (AddTask.IsOpen)
            {
                AddTask.Close();
                e.Cancel = true;
            }

            base.OnBackKeyPress(e);
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

                    ResyncLists();
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

                    ResyncLists();
                });
            }
        }
    }
}