using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using IronCow;
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
            string idStr;

            if (this.NavigationContext.QueryString.TryGetValue("id", out idStr))
            {
                // set current list
                int listId = int.Parse(idStr);

                CurrentList = App.RtmClient.TaskLists.GetById(listId);
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
                    UpdateCurrentList();
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
            App.RtmClient.AddTask(e.Text, true, CurrentList.Id, () =>
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
    }
}