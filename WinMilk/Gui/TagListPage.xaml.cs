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
using WinMilk.Helper;

namespace WinMilk.Gui
{
    public partial class TagListPage : PhoneApplicationPage
    {
        public readonly static DependencyProperty TagsProperty =
            DependencyProperty.Register("Tags", typeof(SortableObservableCollection<string>), typeof(TagListPage),
                new PropertyMetadata((SortableObservableCollection<string>)null));

        public SortableObservableCollection<string> Tags
        {
            get { return (SortableObservableCollection<string>)GetValue(TagsProperty); }
            set { SetValue(TagsProperty, value); }
        }

        public TagListPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            LoadTags();
        }

        public void LoadTags()
        {
            Tags = new SortableObservableCollection<string>();
            var tags = App.RtmClient.GetTasksByTag();
            foreach (var tag in tags)
            {
                Tags.Add(tag.Key);
            }

            Tags.Sort();
        }

        private void TagsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                string currentTag = e.AddedItems[0] as string;
                NavigationService.Navigate(new Uri("/Gui/TagPage.xaml?tag=" + Uri.EscapeUriString(currentTag), UriKind.Relative));
            }
        }
    }
}