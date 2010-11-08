using System;
using System.Windows;
using System.Windows.Input;
using IronCow;
using Microsoft.Phone.Controls;
using WinMilk.Helper;

namespace WinMilk.Gui
{
    public partial class SearchPage : PhoneApplicationPage
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