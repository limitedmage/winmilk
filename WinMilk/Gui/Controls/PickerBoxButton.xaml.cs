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
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace WinMilk.Gui.Controls
{
    public partial class PickerBoxButton : Button
    {
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(ObservableCollection<object>), typeof(PickerBoxButton), 
            new PropertyMetadata(new ObservableCollection<object>(), new PropertyChangedCallback(ItemsChangedCallback)));

        public ObservableCollection<object> Items
        {
            get { return (ObservableCollection<object>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        public static readonly DependencyProperty CurrentItemProperty =
            DependencyProperty.Register("CurrentItem", typeof(object), typeof(PickerBoxButton), new PropertyMetadata(null));

        public object CurrentItem
        {
            get { return GetValue(CurrentItemProperty); }
            set { SetValue(CurrentItemProperty, value); }
        }

        public static readonly DependencyProperty DialogTitleProperty =
            DependencyProperty.Register("DialogTitle", typeof(string), typeof(PickerBoxButton), new PropertyMetadata(""));

        public string DialogTitle
        {
            get { return (string)GetValue(DialogTitleProperty); }
            set { SetValue(DialogTitleProperty, value); }
        }

        private PickerBoxDialog _dialog;

        public PickerBoxButton()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _dialog = new PickerBoxDialog();
            _dialog.ItemSource = Items;
            _dialog.Title = DialogTitle; 
            _dialog.Closed += new EventHandler(dialog_Closed);

            if (CurrentItem == null || !Items.Contains(CurrentItem))
            {
                if (Items.Count > 0)
                {
                    CurrentItem = Items[0];
                }
            }

            _dialog.SelectedItem = (object) CurrentItem;
            _dialog.Show();
        }

        private void dialog_Closed(object sender, EventArgs e)
        {
            var selectedIndex = _dialog.SelectedIndex;
            // Dialog closed. Assign the value to the button
            CurrentItem = Items[selectedIndex];
        }

        private static void ItemsChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                var items = (ObservableCollection<object>)e.NewValue;
                var target = d as PickerBoxButton;
                if (target.CurrentItem == null || !items.Contains(target.CurrentItem))
                {
                    if (items.Count > 0)
                    {
                        target.CurrentItem = items[0];
                    }
                }
            }
        }
    }
}
