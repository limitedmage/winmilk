using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using IronCow;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using WinMilk.Helper;

namespace WinMilk.Gui
{
    public partial class AddTaskPage : PhoneApplicationPage
    {
        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register("IsLoading", typeof(bool), typeof(AddTaskPage),
                new PropertyMetadata((bool)false));

        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        public static readonly DependencyProperty TaskListsProperty =
               DependencyProperty.Register("TaskLists", typeof(ObservableCollection<TaskList>), typeof(AddTaskPage),
                   new PropertyMetadata(new ObservableCollection<TaskList>()));

        public ObservableCollection<TaskList> TaskLists
        {
            get { return (ObservableCollection<TaskList>)GetValue(TaskListsProperty); }
            set { SetValue(TaskListsProperty, value); }
        }

        public AddTaskPage()
        {
            InitializeComponent();
            CreateApplicationBar();

            TaskLists.Clear();
            foreach (TaskList l in App.RtmClient.GetParentableTaskLists(false))
            {
                TaskLists.Add(l);
            }
        }

        public void CreateApplicationBar()
        {
            // Build ApplicationBar with localized strings
            ApplicationBar = new ApplicationBar();

            ApplicationBarIconButton add = new ApplicationBarIconButton(new Uri("/icons/appbar.add.rest.png", UriKind.Relative));
            add.Text = AppResources.AddTaskAppbar;
            add.Click += new EventHandler(Add_Click);
            ApplicationBar.Buttons.Add(add);
        }

        private void Add_Click(object sender, EventArgs e)
        {
            if (!IsLoading)
            {
                StringBuilder smartaddstr = new StringBuilder();

                // task name
                smartaddstr.Append(TaskName.Text);
                smartaddstr.Append(" ");

                // tags
                if (TaskTags.Text.Length > 0)
                {
                    foreach (string s in TaskTags.Text.Split(' '))
                    {
                        if (s.Length > 0)
                        {
                            smartaddstr.Append("#");
                            smartaddstr.Append(s);
                            smartaddstr.Append(" ");
                        }
                    }
                }

                // due date
                if (DueDayRadio.IsChecked.HasValue && DueDayRadio.IsChecked.Value)
                {
                    if (TaskDueDateNoTime.Value.HasValue)
                    {
                        smartaddstr.Append("^");
                        smartaddstr.Append(TaskDueDateNoTime.Value.Value.ToString("yyyy-MM-dd"));
                        smartaddstr.Append(" ");
                    }
                }
                else if (DueTimeRadio.IsChecked.HasValue && DueTimeRadio.IsChecked.Value)
                {
                    if (TaskDueDate.Value.HasValue)
                    {
                        smartaddstr.Append("^");
                        smartaddstr.Append(TaskDueDate.Value.Value.ToString("yyyy-MM-dd"));
                    }

                    if (TaskDueTime.Value.HasValue)
                    {
                        smartaddstr.Append(" @ ");
                        smartaddstr.Append(TaskDueTime.Value.Value.ToString("HH:mm"));
                    }

                    smartaddstr.Append(" ");
                }

                // priority
                if (TaskPriority1.IsChecked.HasValue && TaskPriority1.IsChecked.Value)
                {
                    smartaddstr.Append("!1 ");
                }
                else if (TaskPriority2.IsChecked.HasValue && TaskPriority2.IsChecked.Value)
                {
                    smartaddstr.Append("!2 ");
                }
                else if (TaskPriority3.IsChecked.HasValue && TaskPriority3.IsChecked.Value)
                {
                    smartaddstr.Append("!3 ");
                }

                // Add the task
                IsLoading = true;
                App.RtmClient.AddTask(smartaddstr.ToString(), true, (TaskList.SelectedItem as TaskList).Id, () =>
                {
                    App.RtmClient.CacheTasks(() =>
                    {
                        SmartDispatcher.BeginInvoke(() =>
                        {
                            if (NavigationService.CanGoBack)
                            {
                                NavigationService.GoBack();
                            }
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