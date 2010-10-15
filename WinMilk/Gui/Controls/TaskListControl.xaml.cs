﻿using System;
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

namespace WinMilk.Gui.Controls
{
    public partial class TaskListControl : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {
        public bool HasItems
        {
            get { return this.list.Items.Count > 0; }
        }

        public TaskListControl()
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

            List<RTM.Task> tasks = list.ItemsSource as List<RTM.Task>;
            RTM.Task selectedTask = tasks[list.SelectedIndex];

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