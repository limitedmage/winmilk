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
using System.ComponentModel;
using IronCow;

namespace WinMilk.Gui.Controls
{
    public partial class TaskListControlOld : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {

        public static readonly DependencyProperty TasksProperty =
               DependencyProperty.Register("Tasks", typeof(List<Task>), typeof(TaskListControl),
                   new PropertyMetadata(new List<Task>()));

        public List<Task> Tasks
        {
            get { return (List<Task>)GetValue(TasksProperty); }
            set { SetValue(TasksProperty, value); }
        }

        public bool HasItems
        {
            get { return Tasks.Count > 0; }
        }

        public TaskListControlOld()
        {
            InitializeComponent();
        }

        private void list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox list = sender as ListBox;

            if (list.SelectedIndex == -1)
            {
                return;
            }

            List<Task> tasks = list.ItemsSource as List<Task>;
            Task selectedTask = tasks[list.SelectedIndex];

            PhoneApplicationFrame frame = App.Current.RootVisual as PhoneApplicationFrame;
            frame.Navigate(new Uri("/Gui/TaskDetailsPage.xaml?id=" + selectedTask.Id, UriKind.Relative));

            list.SelectedIndex = -1;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void list_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("HasItems"));
        }
    }
}
