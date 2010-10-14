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

namespace WinMilk.Gui.Controls
{
    public partial class IconButton : UserControl
    {
        #region IconSources

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(string), typeof(IconButton), new PropertyMetadata(string.Empty));
        public string Type 
        {
            get { return (string)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public string IconSource
        {
            get { return "/icons/appbar." + Type + ".rest.png"; }
        }

        public string LightIconSource
        {
            get { return "/icons/light/appbar." + Type + ".rest.png"; }
        }

        #endregion

        #region Theme Selection

        public bool IsDarkTheme
        {
            get
            {
                Color themeColor = (Color)Application.Current.Resources["PhoneForegroundColor"];

                if (themeColor.ToString() == "#FFFFFFFF")
                {
                    return true;
                }

                return false;
            }
        }

        public Visibility LightVisibility
        {
            get
            {
                if (IsDarkTheme) return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Visibility DarkVisibility
        {
            get
            {
                if (IsDarkTheme) return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        #endregion

        #region TextProperty

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(IconButton), new PropertyMetadata(string.Empty));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        #endregion

        #region Events

        public event EventHandler Click;

        private void RaiseClick(RoutedEventArgs e)
        {
            if (Click != null) Click(this, e);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RaiseClick(e);
        }

        #endregion

        public IconButton()
        {
            InitializeComponent();
        }
    }
}
