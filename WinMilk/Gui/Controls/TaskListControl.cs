using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using IronCow;
using Microsoft.Phone.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace WinMilk.Gui.Controls
{
    public class TaskListControl : ListBox, INotifyPropertyChanged
    {
        public bool HasItems
        {
            get 
            {
                if (ItemsSource is IList)
                {
                    return (this.ItemsSource as IList).Count > 0;
                }
                return false;
            }
        }

        public TaskListControl()
            : base()
        {
            this.SelectionChanged += new SelectionChangedEventHandler(list_SelectionChanged);

            this.Loaded += new RoutedEventHandler(TaskListControl_Loaded);

            this.ItemTemplate = App.Current.Resources["TaskTemplate"] as DataTemplate;
        }

        void TaskListControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        ///     Enables a context menu for the control.
        /// </summary>
        /// <param name="callback">Callback which will be called when the context menu is called.</param>
        public void AddContextMenu(ContextMenuSelected callback)
        {
            ContextMenuSelectedEvent = callback;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuitem = new MenuItem() { Header = AppResources.TaskCompleteButton, Name = "Complete" };
            menuitem.Click += new RoutedEventHandler(this.ContextMenuClick);
            contextMenu.Items.Add(menuitem);

            menuitem = new MenuItem() { Header = AppResources.TaskPostponeButton, Name = "Postpone" };
            menuitem.Click += new RoutedEventHandler(this.ContextMenuClick);
            contextMenu.Items.Add(menuitem);

            this.SetValue(ContextMenuService.ContextMenuProperty, contextMenu);
        }

        /// <summary>
        ///     Will provide a callback when a task is selected via a context menu.
        /// </summary>
        /// <param name="menuItem">The name of the context menu item that was selected.</param>
        /// <param name="task">The task that was selected via context menu.</param>
        public delegate void ContextMenuSelected(string menuItem, Task task);

        protected ContextMenuSelected ContextMenuSelectedEvent
        {
            get;
            set;
        }

        private void ContextMenuClick(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;

            if (ContextMenuSelectedEvent != null)
            {
                ContextMenuSelectedEvent(menuItem.Name, MostRecentTaskClick);
            }
        }

        private void list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.SelectedIndex == -1)
            {
                return;
            }

            Task selectedTask = null;

            if (ItemsSource is ObservableCollection<Task>)
            {
                ObservableCollection<Task> tasks = ItemsSource as ObservableCollection<Task>;
                selectedTask = tasks[this.SelectedIndex];
            }
            else if (ItemsSource is TaskListTaskCollection)
            {
                TaskListTaskCollection tasks = ItemsSource as TaskListTaskCollection;
                selectedTask = tasks[this.SelectedIndex];
            }

            if (selectedTask != null)
            {
                PhoneApplicationFrame frame = App.Current.RootVisual as PhoneApplicationFrame;
                frame.Navigate(new Uri("/Gui/TaskDetailsPage.xaml?id=" + selectedTask.Id, UriKind.Relative));
            }

            this.SelectedIndex = -1;
        }

        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (this.PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("HasItems"));

            base.OnItemsChanged(e);
        }

        /// <summary>
        ///     Used to keep track of which context menu item was selected.
        /// </summary>
        private Task MostRecentTaskClick
        {
            get;
            set;
        }

        /// <summary>
        ///     Sets the MostRecentTaskClick property in order to support ContextMenus.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement)
            {
                FrameworkElement frameworkElement = (FrameworkElement)e.OriginalSource;
                if (frameworkElement.DataContext is Task)
                {
                    MostRecentTaskClick = (Task)frameworkElement.DataContext;
                }
            } 
            base.OnMouseLeftButtonDown(e);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class OverdueToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                if ((bool)value) // task is overdue
                {
                    // red accent color
                    return new SolidColorBrush(Color.FromArgb(0xff, 0xe5, 0x14, 0x00));
                }
                else
                {
                    return Application.Current.Resources["PhoneForegroundBrush"];
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class OverdueToBoldConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                if ((bool)value) // task is overdue
                {
                    // show as bold
                    return FontWeights.SemiBold;
                }
                else
                {
                    return FontWeights.Normal;
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
