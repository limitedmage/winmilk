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
using IronCow;
using System.Collections.ObjectModel;

namespace WinMilk.Gui
{
    public partial class SearchPage : PhoneApplicationPage
    {
        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register("IsLoading", typeof(bool), typeof(SearchPage),
                new PropertyMetadata((bool)false));

        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        public ObservableCollection<Task> ResultTasks
        {
            get { return (ObservableCollection<Task>)GetValue(ResultTasksProperty); }
            set { SetValue(ResultTasksProperty, value); }
        }

        public static readonly DependencyProperty ResultTasksProperty =
               DependencyProperty.Register("ResultTasks", typeof(ObservableCollection<Task>), typeof(SearchPage),
                   new PropertyMetadata(new ObservableCollection<Task>()));


        public SearchPage()
        {
            InitializeComponent();
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            IsLoading = true;
            App.RtmClient.GetTasks(SearchQueryTextBox.Text, tasks => 
            {
                Dispatcher.BeginInvoke(() =>
                {
                    ResultTasks.Clear();
                    foreach (Task t in tasks)
                    {
                        ResultTasks.Add(t);
                    }
                    IsLoading = false;
                });
            });
        }
    }
}