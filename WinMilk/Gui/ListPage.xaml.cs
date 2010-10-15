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

namespace WinMilk.Gui
{
    public partial class ListPage : PhoneApplicationPage
    {
        public static readonly DependencyProperty ListNameProperty =
               DependencyProperty.Register("ListName", typeof(string), typeof(ListPage),
                   new PropertyMetadata(string.Empty));

        public string ListName
        {
            get { return (string)GetValue(ListNameProperty); }
            set { SetValue(ListNameProperty, value); }
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
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            string idStr;

            if (this.NavigationContext.QueryString.TryGetValue("id", out idStr))
            {
                LoadList(idStr);
            }

            base.OnNavigatedTo(e);
        }

        private void LoadList(string idStr)
        {
            int id = int.Parse(idStr);

            IsLoading = true;

            App.Rest.GetList(id, (RTM.TaskList l) =>
            {
                ListName = l.Name;

                App.Rest.GetTasksInList(l, (List<RTM.Task> tasks) =>
                {
                    if (l.SortOrder == RTM.TaskListSortOrder.Date)
                    {
                        tasks.Sort(RTM.Task.CompareByDate);
                    }
                    else if (l.SortOrder == RTM.TaskListSortOrder.Priority)
                    {
                        tasks.Sort(RTM.Task.CompareByPriority);
                    }
                    else
                    {
                        tasks.Sort(RTM.Task.CompareByName);
                    }

                    Tasks.list.ItemsSource = tasks;

                    IsLoading = false;
                });
            });
        }

        private void CreateApplicationBar()
        {
            // Build ApplicationBar with localized strings
            ApplicationBar = new ApplicationBar();

            ApplicationBarIconButton add = new ApplicationBarIconButton(new Uri("/icons/appbar.add.rest.png", UriKind.Relative));
            add.Text = AppResources.AddTaskAppbar;
            //add.Click += new EventHandler(add_Click);
            ApplicationBar.Buttons.Add(add);

            ApplicationBarIconButton sync = new ApplicationBarIconButton(new Uri("/icons/appbar.refresh.rest.png", UriKind.Relative));
            sync.Text = AppResources.SyncAppbar;
            //sync.Click += new EventHandler(sync_Click);
            ApplicationBar.Buttons.Add(sync);

            ApplicationBarMenuItem pin = new ApplicationBarMenuItem(AppResources.PinAppbar);
            //settings.Click += new EventHandler(settings_Click);
            ApplicationBar.MenuItems.Add(pin);
        }
    }
}