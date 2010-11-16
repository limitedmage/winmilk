using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Linq;
using IronCow;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using WinMilk.Helper;

namespace WinMilk.Gui
{
    public partial class EditTaskPage : PhoneApplicationPage
    {
        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register("IsLoading", typeof(bool), typeof(EditTaskPage),
                new PropertyMetadata((bool)false));

        private bool loadedDetails = false;

        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        public static readonly DependencyProperty TaskListsProperty =
               DependencyProperty.Register("TaskLists", typeof(ObservableCollection<TaskList>), typeof(EditTaskPage),
                   new PropertyMetadata(new ObservableCollection<TaskList>()));

        public ObservableCollection<TaskList> TaskLists
        {
            get { return (ObservableCollection<TaskList>)GetValue(TaskListsProperty); }
            set { SetValue(TaskListsProperty, value); }
        }

        public static readonly DependencyProperty TaskProperty =
            DependencyProperty.Register("Task", typeof(Task), typeof(EditTaskPage), new PropertyMetadata(new Task()));

        private Task CurrentTask
        {
            get { return (Task)GetValue(TaskProperty); }
            set { SetValue(TaskProperty, value); }
        }

        public EditTaskPage()
        {
            InitializeComponent();
            CreateApplicationBar();
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (TaskList.ListPickerMode == ListPickerMode.Expanded)
            {
                TaskList.ListPickerMode = ListPickerMode.Normal;
                e.Cancel = true;
            }

            base.OnBackKeyPress(e);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (!loadedDetails)
            {
                LoadTaskDetails();
                loadedDetails = true;
            }
            base.OnNavigatedTo(e);
        }

        public void LoadTaskDetails()
        {
            TaskLists.Clear();
            foreach (TaskList l in App.RtmClient.GetParentableTaskLists(false))
            {
                TaskLists.Add(l);
            }
            TaskList.ItemsSource = TaskLists;

            string idStr;

            if (this.NavigationContext.QueryString.TryGetValue("id", out idStr))
            {
                int id = int.Parse(idStr);

                CurrentTask = App.RtmClient.GetTask(id);

                TaskName.Text = CurrentTask.Name;
                if (CurrentTask.DueDateTime.HasValue)
                {
                    if (CurrentTask.HasDueTime)
                    {
                        TaskDueTime.Value = CurrentTask.DueDateTime;
                        TaskDueDate.Value = CurrentTask.DueDateTime;
                        DueTimeRadio.IsChecked = true;
                    }
                    else
                    {
                        TaskDueDateNoTime.Value = CurrentTask.DueDateTime;
                        DueDayRadio.IsChecked = true;
                    }
                }
                
                TaskList.SelectedItem = CurrentTask.Parent;

                TaskTags.Text = CurrentTask.TagsString;

                switch (CurrentTask.Priority)
                {
                    case TaskPriority.One:
                        TaskPriority1.IsChecked = true;
                        break;
                    case TaskPriority.Two:
                        TaskPriority2.IsChecked = true;
                        break;
                    case TaskPriority.Three:
                        TaskPriority3.IsChecked = true;
                        break;
                }
            }
        }

        public void CreateApplicationBar()
        {
            // Build ApplicationBar with localized strings
            ApplicationBar = new ApplicationBar();

            ApplicationBarIconButton save = new ApplicationBarIconButton(new Uri("/icons/appbar.save.rest.png", UriKind.Relative));
            save.Text = AppResources.EditTaskSaveButton;
            save.Click += new EventHandler(Save_Click);
            ApplicationBar.Buttons.Add(save);
        }

        private void Save_Click(object sender, EventArgs e)
        {
            if (!IsLoading)
            {
                IsLoading = true;

                /*
                 * TODO: Clean this mess up
                 * 
                 * This is, without a doubt, the most horrible mess
                 * code I have ever written. u__u
                 * 
                 * And yes, this IS C#, not Lisp or some other
                 * weid esoteric functional language.
                 */

                // do logic.....
                // change name
                SmartDispatcher.BeginInvoke(() =>
                {
                    CurrentTask.ChangeName(TaskName.Text, () =>
                    {
                        // change priority
                        SmartDispatcher.BeginInvoke(() =>
                        {
                            TaskPriority p;
                            if (TaskPriority1.IsChecked.HasValue && TaskPriority1.IsChecked.Value) p = TaskPriority.One;
                            else if (TaskPriority2.IsChecked.HasValue && TaskPriority2.IsChecked.Value) p = TaskPriority.Two;
                            else if (TaskPriority3.IsChecked.HasValue && TaskPriority3.IsChecked.Value) p = TaskPriority.Three;
                            else p = TaskPriority.None;
                            CurrentTask.ChangePriority(p, () =>
                            {
                                SmartDispatcher.BeginInvoke(() =>
                                {
                                    // change due
                                    DateTime? due;
                                    bool hasTime;
                                    if (DueDayRadio.IsChecked.HasValue && DueDayRadio.IsChecked.Value)
                                    {
                                        hasTime = false;
                                        due = TaskDueDateNoTime.Value.Value.Date;
                                    }
                                    else if (DueTimeRadio.IsChecked.HasValue && DueTimeRadio.IsChecked.Value)
                                    {
                                        hasTime = true;
                                        due = TaskDueDate.Value.Value.Date + TaskDueTime.Value.Value.TimeOfDay;
                                    }
                                    else
                                    {
                                        hasTime = false;
                                        due = null;
                                    }

                                    CurrentTask.ChangeDue(due, hasTime, () =>
                                    {
                                        // change tags
                                        SmartDispatcher.BeginInvoke(() =>
                                        {
                                            string[] tags = TaskTags.Text.Split(new char[] {' ', ','}, StringSplitOptions.RemoveEmptyEntries);
                                            CurrentTask.ChangeTags(tags, () =>
                                            {
                                                SmartDispatcher.BeginInvoke(() =>
                                                {
                                                    // change list
                                                    CurrentTask.ChangeList((TaskList)TaskList.SelectedItem, () =>
                                                    {
                                                        // done editing task
                                                        // now sync all tasks
                                                        App.RtmClient.CacheTasks(() =>
                                                        {
                                                            // now we can go back to task page
                                                            SmartDispatcher.BeginInvoke(() =>
                                                            {
                                                                IsLoading = false;
                                                                if (NavigationService.CanGoBack) NavigationService.GoBack();
                                                            });
                                                        });
                                                    });
                                                });
                                            });
                                        });
                                    });
                                });
                            });
                        });
                    });
                });
            }
        }

        private void NoDueRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (TaskDueDateNoTime != null) TaskDueDateNoTime.Visibility = Visibility.Collapsed;
            if (TaskDueDateTime != null) TaskDueDateTime.Visibility = Visibility.Collapsed;
        }

        private void DueDayRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (TaskDueDateNoTime != null) TaskDueDateNoTime.Visibility = Visibility.Visible;
            if (TaskDueDateTime != null) TaskDueDateTime.Visibility = Visibility.Collapsed;
        }

        private void DueTimeRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (TaskDueDateNoTime != null) TaskDueDateNoTime.Visibility = Visibility.Collapsed;
            if (TaskDueDateTime != null) TaskDueDateTime.Visibility = Visibility.Visible;
        }


    }
}