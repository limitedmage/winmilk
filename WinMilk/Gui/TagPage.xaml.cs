using System.Collections.ObjectModel;
using System.Windows;
using IronCow;
using Microsoft.Phone.Controls;

namespace WinMilk.Gui
{
    public partial class TagPage : PhoneApplicationPage
    {
        public static readonly DependencyProperty CurrentTagProperty =
               DependencyProperty.Register("CurrentTag", typeof(TagList), typeof(TagPage),
                   new PropertyMetadata((TagList)null));

        public TagList CurrentTag
        {
            get { return (TagList)GetValue(CurrentTagProperty); }
            set { SetValue(CurrentTagProperty, value); }
        }

        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register("IsLoading", typeof(bool), typeof(TagPage),
                new PropertyMetadata((bool)false));

        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        public TagPage()
        {
            InitializeComponent();

            TaskList.AddContextMenu(TaskListContextMenuClick);
        }

        public void LoadTag()
        {
            var tags = App.RtmClient.GetTasksByTag();
            
            // select current tag
            string curr;

            if (this.NavigationContext.QueryString.TryGetValue("tag", out curr))
            {
                // find task by name, and select it
                foreach (var l in tags)
                {
                    if (l.Key == curr)
                    {
                        CurrentTag = new TagList { Tag = l.Key, Tasks = l.Value };
                        break;
                    }
                }
            }
        }

        /// <summary>
        ///     This event is fired when a context menu action for an item is selected (Complete/Postpone).
        /// </summary>
        private void TaskListContextMenuClick(string menuItem, Task task)
        {
            // now that we have the associated task, we can take action on it.
            if (menuItem == "Complete")
            {
                task.Complete(() =>
                {
                    // Task was marked as complete, just remove it from the list.
                    Dispatcher.BeginInvoke(() =>
                    {
                        CurrentTag.Tasks.Remove(task);
                    });
                });
            }
            else if (menuItem == "Postpone")
            {
                task.Postpone(() =>
                {
                });
            }
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadTag();
        }

        public class TagList
        {
            public string Tag { get; set; }
            public ObservableCollection<Task> Tasks { get; set; }
        }
    }
}