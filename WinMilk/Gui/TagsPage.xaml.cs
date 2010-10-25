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

namespace WinMilk.Gui
{
    public partial class TagsPage : PhoneApplicationPage, INotifyPropertyChanged
    {
        public static bool sReload = true;

        public static readonly DependencyProperty TagsProperty =
               DependencyProperty.Register("Tags", typeof(ObservableCollection<TagList>), typeof(TagsPage),
                   new PropertyMetadata(new ObservableCollection<TagList>()));

        public ObservableCollection<TagList> Tags
        {
            get { return (ObservableCollection<TagList>)GetValue(TagsProperty); }
            set { SetValue(TagsProperty, value); }
        }

        public static readonly DependencyProperty CurrentTagProperty =
               DependencyProperty.Register("CurrentTag", typeof(TagList), typeof(TagsPage),
                   new PropertyMetadata((TagList)null, new PropertyChangedCallback(OnCurrentTagChanged)));

        public TagList CurrentTag
        {
            get { return (TagList)GetValue(CurrentTagProperty); }
            set { SetValue(CurrentTagProperty, value); }
        }

        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register("IsLoading", typeof(bool), typeof(TagsPage),
                new PropertyMetadata((bool)false));

        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        public TagsPage()
        {
            InitializeComponent();
        }

        public void LoadAllTags()
        {
            Tags = new ObservableCollection<TagList>();
            var tags = App.RtmClient.GetTasksByTag();
            foreach (var tag in tags)
            {
                Tags.Add(new TagList { Tag = tag.Key, Tasks = tag.Value });
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
            TagsPage target = d as TagsPage;
            TagList oldTag = e.OldValue as TagList;
            TagList newTag = target.CurrentTag;
            target.OnCurrentTagChanged(oldTag, newTag);
        }

        private void OnCurrentTagChanged(TagList oldTag, TagList newTag)
        {
            if (newTag.Tag != oldTag.Tag)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    TagsPivot.SelectedItem = newTag;
                });
            }
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

        public class TagList
        {
            public string Tag { get; set; }
            public ObservableCollection<Task> Tasks { get; set; }
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAllTags();
        }
    }
}