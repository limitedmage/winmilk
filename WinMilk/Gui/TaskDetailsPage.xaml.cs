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
using Microsoft.Phone.Tasks;

namespace WinMilk.Gui
{
    public partial class TaskDetailsPage : PhoneApplicationPage
    {

        #region Task Property

        public static readonly DependencyProperty TaskProperty =
            DependencyProperty.Register("Task", typeof(Task), typeof(TaskDetailsPage), new PropertyMetadata(new Task()));

        private Task CurrentTask
        {
            get { return (Task)GetValue(TaskProperty); }
            set { SetValue(TaskProperty, value); }
        }

        #endregion

        #region Construction and Navigation

        public TaskDetailsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            string idStr;

            if (this.NavigationContext.QueryString.TryGetValue("id", out idStr))
            {
                int id = int.Parse(idStr);

                CurrentTask = App.RtmClient.GetTask(id);
            }
        }

        #endregion

        #region Event Handlers

        private void CompleteButton_Click(object sender, EventArgs e)
        {
            if (CurrentTask != null)
            {
                CurrentTask.Complete(() =>
                {
                    this.NavigationService.GoBack();
                });
            }
        }

        private void PostponeButton_Click(object sender, EventArgs e)
        {
            if (CurrentTask != null)
            {
                CurrentTask.Postpone(() =>
                {
                    this.NavigationService.GoBack();
                });
            }
        }

        private void EditButton_Click(object sender, EventArgs e)
        {

        }

        private void AddNoteButton_Click(object sender, EventArgs e)
        {
            
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask page = new WebBrowserTask();
            page.URL = CurrentTask.Url;
            page.Show();
        }

        #endregion
    }
}