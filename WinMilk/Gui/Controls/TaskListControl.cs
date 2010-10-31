using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.ComponentModel;
using System.Collections;
using System.Collections.ObjectModel;
using IronCow;
using Delay;

namespace WinMilk.Gui.Controls
{
    public class TaskListControl : DeferredLoadListBox, INotifyPropertyChanged
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
            this.ItemContainerStyle = App.Current.Resources["TaskContainterStyle"] as Style;
            this.ItemsPanel = App.Current.Resources["TaskPanelTemplate"] as ItemsPanelTemplate;
        }

        void TaskListControl_Loaded(object sender, RoutedEventArgs e)
        {
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

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
