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
using WinMilk.Helper;

namespace WinMilk.Gui
{
    public partial class SearchPage : Clarity.Phone.Controls.AnimatedBasePage
    {
        private bool loaded;

        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register("IsLoading", typeof(bool), typeof(SearchPage),
                new PropertyMetadata((bool)false));

        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        public SortableObservableCollection<Task> ResultTasks
        {
            get { return (SortableObservableCollection<Task>)GetValue(ResultTasksProperty); }
            set { SetValue(ResultTasksProperty, value); }
        }

        public static readonly DependencyProperty ResultTasksProperty =
               DependencyProperty.Register("ResultTasks", typeof(SortableObservableCollection<Task>), typeof(SearchPage),
                   new PropertyMetadata(new SortableObservableCollection<Task>()));


        public SearchPage()
        {
            InitializeComponent();
            loaded = false;
            AnimationContext = LayoutRoot;
        }

        private void DoSearch()
        {
            try
            {
                var res = App.RtmClient.SearchTasksLocally(SearchQueryTextBox.Text);
                ResultTasks.Clear();
                foreach (Task t in res)
                {
                    ResultTasks.Add(t);
                }
                ResultTasks.Sort();
            }
            catch (Exception)
            {
                // ignore any exception while processing 
            }
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            DoSearch();
        }

        private void SearchQueryTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // remove focus from text box to close keyboard
                this.Focus();
                DoSearch();
            }
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!loaded)
            {
                ResultTasks.Clear();
                SearchQueryTextBox.Focus();
                loaded = true;
            }
        }
    }
}