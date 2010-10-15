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

namespace WinMilk.Gui.Controls
{
    public partial class TaskListControl : System.Windows.Controls.UserControl
    {
        public static readonly DependencyProperty ShowScrollProperty =
            DependencyProperty.Register("ShowScroll", typeof(bool), typeof(TaskListControl),
                new PropertyMetadata((bool)true));

        public bool ShowScroll
        {
            get { return (bool)GetValue(ShowScrollProperty); }
            set { SetValue(ShowScrollProperty, value); }
        }

        public TaskListControl()
        {
            InitializeComponent();
            ShowScroll = true;
        }

        private void list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox list = sender as ListBox;

            if (list.SelectedIndex == -1)
            {
                return;
            }

            List<RTM.Task> tasks = list.ItemsSource as List<RTM.Task>;
            RTM.Task selectedTask = tasks[list.SelectedIndex];

            PhoneApplicationFrame frame = App.Current.RootVisual as PhoneApplicationFrame;
            frame.Navigate(new Uri("/Gui/TaskDetailsPage.xaml?id=" + selectedTask.Id, UriKind.Relative));
            

            list.SelectedIndex = -1;
        }
    }
}
