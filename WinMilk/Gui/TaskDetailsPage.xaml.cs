using System;
using System.Windows;
using System.Collections.Generic;

using IronCow;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using WinMilk.Helper;

namespace WinMilk.Gui
{
    public partial class TaskDetailsPage : PhoneApplicationPage
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
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            string id;

            if (this.NavigationContext.QueryString.TryGetValue("id", out id))
            {
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

            ApplicationBarIconButton edit = new ApplicationBarIconButton(new Uri("/icons/appbar.edit.rest.png", UriKind.Relative));
            edit.Text = AppResources.TaskEditButton;
            edit.Click += new EventHandler(EditButton_Click);
            ApplicationBar.Buttons.Add(edit);

            ApplicationBarIconButton delete = new ApplicationBarIconButton(new Uri("/icons/appbar.delete.rest.png", UriKind.Relative));
            delete.Text = AppResources.TaskDeleteButton;
            delete.Click += new EventHandler(DeleteButton_Click);
            ApplicationBar.Buttons.Add(delete);

            ApplicationBarMenuItem sendEmail = new ApplicationBarMenuItem(AppResources.TaskShareEmailButton);
            sendEmail.Click += new EventHandler(SendEmailButton_Click);
            ApplicationBar.MenuItems.Add(sendEmail);

            ApplicationBarMenuItem sendMessage = new ApplicationBarMenuItem(AppResources.TaskShareMessageButton);
            sendMessage.Click += new EventHandler(SendMessageButton_Click);
            ApplicationBar.MenuItems.Add(sendMessage);
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
                    App.RtmClient.CacheTasks(() =>
                    {
                        Dispatcher.BeginInvoke(() =>
                        {
                            IsLoading = false;
                            this.NavigationService.GoBack();
                        });
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
                    App.RtmClient.CacheTasks(() =>
                    {
                        SmartDispatcher.BeginInvoke(() =>
                        {
                            IsLoading = false;
                            this.NavigationService.GoBack();
                        });
                    });
                });
            }
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            if (CurrentTask != null && !IsLoading)
            {
                NavigationService.Navigate(new Uri("/Gui/EditTaskPage.xaml?id=" + Uri.EscapeUriString(CurrentTask.Id.ToString()), UriKind.Relative));
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (CurrentTask != null && !IsLoading)
            {
                MessageBoxResult delete = MessageBox.Show(AppResources.TaskDeleteConfirmText, AppResources.TaskDeleteConfirmTitle, MessageBoxButton.OKCancel);

                if (delete == MessageBoxResult.OK)
                {
                    IsLoading = true;

                    CurrentTask.Delete(() =>
                    {
                        App.RtmClient.CacheTasks(() =>
                        {
                            SmartDispatcher.BeginInvoke(() =>
                            {
                                IsLoading = false;
                                this.NavigationService.GoBack();
                            });
                        });
                    });
                }
            }
        }

        private void SendEmailButton_Click(object sender, EventArgs e)
        {
            EmailComposeTask emailComposeTask = new EmailComposeTask();
            emailComposeTask.Body = GenerateTaskMessage();
            emailComposeTask.Subject = AppResources.TaskShareTask + ": " + CurrentTask.Name;
            emailComposeTask.Show();
        }

        private void SendMessageButton_Click(object sender, EventArgs e)
        {
            SmsComposeTask smsComposeTask = new SmsComposeTask();
            smsComposeTask.Body = GenerateTaskMessage();
            smsComposeTask.Show();
        }

        /// <summary>
        ///     Creates a message with line breaks using all of the task information.
        ///     This includes all notes that are part of the task.
        /// </summary>
        /// <returns></returns>
        private string GenerateTaskMessage()
        {
            List<string> output = new List<string>();
            output.Add(AppResources.TaskShareTask + ": " + CurrentTask.Name);
            if (CurrentTask.DueDateTime.HasValue)
            {
                output.Add(AppResources.TaskShareDue + ": " + CurrentTask.LongDueDateString);
            }
            output.Add(AppResources.TaskShareList + ": " + CurrentTask.List);
            if (CurrentTask.HasTags)
            {
                output.Add(AppResources.TaskShareTags + ": " + CurrentTask.TagsString);
            }
            if (CurrentTask.HasUrl)
            {
                output.Add(AppResources.TaskShareUrl + ": " + CurrentTask.Url);
            }
            if (CurrentTask.Notes.Count > 0)
            {
                output.Add(AppResources.TaskShareNotes + ":");
                int noteNumber = 0;
                foreach (TaskNote note in CurrentTask.Notes)
                {
                    //
                    // Output notes with divider between each one.
                    //
                    noteNumber++;
                    if (noteNumber > 1)
                    {
                        output.Add("-----");
                    }
                    if (!string.IsNullOrEmpty(note.Title))
                    {
                        output.Add(note.Title);
                    }
                    if (!string.IsNullOrEmpty(note.Body))
                    {
                        output.Add(note.Body);
                    }
                }
            }

            string formattedOutput = "";
            foreach (string s in output)
            {
                formattedOutput += s + Environment.NewLine;
            }
            return formattedOutput;
        }

        /// PUNTED TO V2 ///
        /*
        private void AddNoteButton_Click(object sender, EventArgs e)
        {
            
        }
        */

        private void Url_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask page = new WebBrowserTask();
            page.URL = CurrentTask.Url;
            page.Show();
        }

        private void AddNoteButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Gui/EditNotePage.xaml?action=add&task=" + CurrentTask.Id, UriKind.Relative));
        }

        #endregion
    }
}