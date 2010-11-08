using System;
using System.Windows;
using System.Windows.Controls;

namespace WinMilk.Gui.Controls
{
    public partial class BigIconButton : UserControl
    {
        #region IconSources

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(string), typeof(BigIconButton), new PropertyMetadata(string.Empty));
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

        #region TextProperty

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(BigIconButton), new PropertyMetadata(string.Empty));

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

        public BigIconButton()
        {
            InitializeComponent();
        }
    }
}
