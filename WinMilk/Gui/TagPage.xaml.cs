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
using System.Collections.ObjectModel;
using IronCow;
using System.ComponentModel;
using WinMilk.Helper;

namespace WinMilk.Gui
{
    public partial class TagPage : PhoneApplicationPage, INotifyPropertyChanged
    {
        private bool reload;

        public static readonly DependencyProperty TagsProperty =
               DependencyProperty.Register("Tags", typeof(SortableObservableCollection<TagList>), typeof(TagPage),
                   new PropertyMetadata(new SortableObservableCollection<TagList>()));

        public SortableObservableCollection<TagList> Tags
        {
            get { return (SortableObservableCollection<TagList>)GetValue(TagsProperty); }
            set { SetValue(TagsProperty, value); }
        }

        public static readonly DependencyProperty CurrentTagProperty =
               DependencyProperty.Register("CurrentTag", typeof(TagList), typeof(TagPage),
                   new PropertyMetadata((TagList)null, new PropertyChangedCallback(OnCurrentTagChanged)));

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
            reload = true;
        }

        public void LoadAllTags()
        {
            if (reload)
            {
                Tags = new SortableObservableCollection<TagList>();
                var tags = App.RtmClient.GetTasksByTag();
                foreach (var tag in tags)
                {
                    Tags.Add(new TagList { Tag = tag.Key, Tasks = tag.Value });
                }

                Tags.Sort();

                // select current tag
                string curr;

                if (this.NavigationContext.QueryString.TryGetValue("tag", out curr))
                {
                    // find task by name, and select it
                    foreach (TagList l in Tags)
                    {
                        if (l.Tag == curr)
                        {
                            CurrentTag = l;
                            break;
                        }
                    }
                }

                reload = false;
            }
        }

        private void TagsPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (e.AddedItems.Count == 0)
            //{
            //    return;
            //}

            //if (e.AddedItems[0] is TaskList)
            //{
            //    CurrentList = e.AddedItems[0] as TaskList;

            //    if (CurrentList.Tasks == null || CurrentList.Tasks.Count == 0)
            //    {
            //        LoadList(CurrentList);
            //    }
            //}
        }

        private static void OnCurrentTagChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TagPage target = d as TagPage;
            TagList oldTag = e.OldValue as TagList;
            TagList newTag = target.CurrentTag;
            target.OnCurrentTagChanged(oldTag, newTag);
        }

        private void OnCurrentTagChanged(TagList oldTag, TagList newTag)
        {
            Dispatcher.BeginInvoke(() =>
            {
                TagsPivot.SelectedItem = newTag;
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                });
            }
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAllTags();
        }

        public class TagList : IComparable<TagList>
        {
            public string Tag { get; set; }
            public ObservableCollection<Task> Tasks { get; set; }

            public int CompareTo(TagList other)
            {
                return this.Tag.CompareTo(other.Tag);
            }
        }
    }
}