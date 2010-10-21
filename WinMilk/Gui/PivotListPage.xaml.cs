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

namespace WinMilk.Gui
{
    public partial class PivotListPage : PhoneApplicationPage
    {
        public static readonly DependencyProperty ListsProperty =
               DependencyProperty.Register("Lists", typeof(ObservableCollection<TaskList>), typeof(PivotListPage),
                   new PropertyMetadata((ObservableCollection<TaskList>)null));

        public ObservableCollection<TaskList> Lists
        {
            get { return (ObservableCollection<TaskList>)GetValue(ListsProperty); }
            set { SetValue(ListsProperty, value); }
        }

        public static readonly DependencyProperty CurrentListProperty =
               DependencyProperty.Register("CurrentList", typeof(TaskList), typeof(PivotListPage),
                   new PropertyMetadata(new TaskList(), new PropertyChangedCallback(OnCurrentListChanged)));

        public TaskList CurrentList
        {
            get { return (TaskList)GetValue(CurrentListProperty); }
            set { SetValue(CurrentListProperty, value); }
        }

        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register("IsLoading", typeof(bool), typeof(PivotListPage),
                new PropertyMetadata((bool)false));

        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        public PivotListPage()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAllLists();

            CreateApplicationBar();
        }
        
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private void LoadAllLists()
        {
            //IsLoading = true;

            Lists = new ObservableCollection<TaskList>();
            foreach (TaskList list in App.RtmClient.TaskLists)
            {
                Lists.Add(list);
            }

            string idStr;

            if (this.NavigationContext.QueryString.TryGetValue("id", out idStr))
            {
                // set current list
                int listId = int.Parse(idStr);

                // find list by id, and select it
                foreach (TaskList l in Lists)
                {
                    if (l.Id == listId)
                    {
                        Dispatcher.BeginInvoke(() =>
                        {
                            CurrentList = l;
                        });
                        break;
                    }
                }
            }
        }

        private void LoadList(TaskList list)
        {
            IsLoading = true;

            list.SyncTasks(() => 
            {
                Dispatcher.BeginInvoke(() =>
                {
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

        private void ListsPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
            {
                return;
            }

            if (e.AddedItems[0] is TaskList)
            {
                TaskList selectedList = e.AddedItems[0] as TaskList;

                if (selectedList.Tasks == null || selectedList.Tasks.Count == 0)
                {
                    LoadList(selectedList);
                }
            }
        }

        private static void OnCurrentListChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PivotListPage target = d as PivotListPage;
            TaskList oldList = e.OldValue as TaskList;
            TaskList newList = target.CurrentList;
            target.OnCurrentListChanged(oldList, newList);
        }

        private void OnCurrentListChanged(TaskList oldList, TaskList newList)
        {
            if (newList != oldList)
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    ListsPivot.SelectedItem = newList;
                }));
            }
        }
    }
}