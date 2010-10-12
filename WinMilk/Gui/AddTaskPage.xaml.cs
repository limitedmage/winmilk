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

namespace WinMilk.Gui
{
    public partial class AddTaskPage : PhoneApplicationPage
    {
        public AddTaskPage()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.NavigationService.GoBack();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {

        }

        private void NoDueRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (taskDueDateNoTime != null) taskDueDateNoTime.Visibility = Visibility.Collapsed;
            if (taskDueDateTime != null)   taskDueDateTime.Visibility = Visibility.Collapsed;
        }

        private void DueDayRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (taskDueDateNoTime != null) taskDueDateNoTime.Visibility = Visibility.Visible;
            if (taskDueDateTime != null) taskDueDateTime.Visibility = Visibility.Collapsed;
        }

        private void DueTimeRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (taskDueDateNoTime != null) taskDueDateNoTime.Visibility = Visibility.Collapsed;
            if (taskDueDateTime != null) taskDueDateTime.Visibility = Visibility.Visible;
        }

        private void taskList_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
        	// TODO: Add event handler implementation here.
			
            
        }
    }
}