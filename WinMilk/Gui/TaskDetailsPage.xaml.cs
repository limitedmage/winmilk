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
using Microsoft.Phone.Shell;
using Clarity.Phone.Controls;

namespace WinMilk.Gui
{
    public partial class TaskDetailsPage : AnimatedBasePage
    {
        #region IsLoading Property

        public static bool sReload = true;

        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register("IsLoading", typeof(bool), typeof(TaskDetailsPage),
                new PropertyMetadata((bool)false));

        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        #endregion

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

            AnimationContext = LayoutRoot;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            string idStr;

            if (this.NavigationContext.QueryString.TryGetValue("id", out idStr))
            {
                int id = int.Parse(idStr);

                CurrentTask = App.RtmClient.GetTask(id);
            }

            CreateApplicationBar();
        }

        private void CreateApplicationBar()
        {
            // Build ApplicationBar with localized strings
            ApplicationBar = new ApplicationBar();

            ApplicationBarIconButton complete = new ApplicationBarIconButton(new Uri("/icons/appbar.check.rest.png", UriKind.Relative));
            complete.Text = AppResources.TaskCompleteButton;
            complete.Click += new EventHandler(CompleteButton_Click);
            ApplicationBar.Buttons.Add(complete);

            ApplicationBarIconButton postpone = new ApplicationBarIconButton(new Uri("/icons/appbar.next.rest.png", UriKind.Relative));
            postpone.Text = AppResources.TaskPostponeButton;
            postpone.Click += new EventHandler(PostponeButton_Click);
            ApplicationBar.Buttons.Add(postpone);
        }

        #endregion

        #region Event Handlers

        private void CompleteButton_Click(object sender, EventArgs e)
        {
            if (CurrentTask != null && !IsLoading)
            {
                IsLoading = true;

                CurrentTask.Complete(() =>
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        PivotLandingPage.sReload = true;
                        IsLoading = false;
                        this.NavigationService.GoBack();
                    });
                });
            }
        }

        private void PostponeButton_Click(object sender, EventArgs e)
        {
            if (CurrentTask != null  && !IsLoading)
            {
                IsLoading = true;

                CurrentTask.Postpone(() =>
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        PivotLandingPage.sReload = true;
                        IsLoading = false;
                        this.NavigationService.GoBack();
                    });
                });
            }
        }

        /// PUNTED TO V2 ///
        /*
        private void EditButton_Click(object sender, EventArgs e)
        {

        }

        private void AddNoteButton_Click(object sender, EventArgs e)
        {
            
        }
        */

        private void Url_Click(object sender, MouseButtonEventArgs e)
        {
            WebBrowserTask page = new WebBrowserTask();
            page.URL = CurrentTask.Url;
            page.Show();
        }

        #endregion
    }
}