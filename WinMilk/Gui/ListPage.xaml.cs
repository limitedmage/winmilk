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

        public ListPage()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            CreateApplicationBar();

            string idStr;

            if (this.NavigationContext.QueryString.TryGetValue("id", out idStr))
            {
                int id = int.Parse(idStr);

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
                    });
                });
            }
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

            ApplicationBarMenuItem settings = new ApplicationBarMenuItem(AppResources.SettingsAppbar);
            //settings.Click += new EventHandler(settings_Click);
            ApplicationBar.MenuItems.Add(settings);

            ApplicationBarMenuItem logout = new ApplicationBarMenuItem(AppResources.LogoutAppbar);
            //logout.Click += new EventHandler(logout_Click);
            ApplicationBar.MenuItems.Add(logout);

            ApplicationBarMenuItem about = new ApplicationBarMenuItem(AppResources.AboutAppbar);
            //about.Click += new EventHandler(about_Click);
            ApplicationBar.MenuItems.Add(about);
        }
    }
}