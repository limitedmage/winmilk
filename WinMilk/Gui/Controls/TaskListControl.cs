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

namespace WinMilk.Gui.Controls
{
    public class TaskListControl : ListBox, INotifyPropertyChanged
    {
        public bool HasItems
        {
            get { return (this.ItemsSource as IList).Count > 0; }
        }

        public TaskListControl()
            : base()
        {
            this.SelectionChanged += new SelectionChangedEventHandler(list_SelectionChanged);

            this.Loaded += new RoutedEventHandler(TaskListControl_Loaded);

            this.ItemTemplate = App.Current.Resources["TaskTemplate"] as DataTemplate;

            this.SizeChanged += new SizeChangedEventHandler(list_SizeChanged);

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

            ObservableCollection<Task> tasks = this.ItemsSource as ObservableCollection<Task>;
            Task selectedTask = tasks[this.SelectedIndex];

            PhoneApplicationFrame frame = App.Current.RootVisual as PhoneApplicationFrame;
            frame.Navigate(new Uri("/Gui/TaskDetailsPage.xaml?id=" + selectedTask.Id, UriKind.Relative));

            this.SelectedIndex = -1;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void list_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("HasItems"));
        }
    }
}
