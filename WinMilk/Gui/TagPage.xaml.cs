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
    public partial class TagPage : Clarity.Phone.Controls.AnimatedBasePage
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

            AnimationContext = LayoutRoot;
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