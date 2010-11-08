using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Phone.Controls;

namespace WinMilk.Gui.Controls
{
    public delegate void SubmitEventHandler(object sentder, SubmitEventArgs e);

    public class SubmitEventArgs : EventArgs
    {
        public string Text { get; set; }

        public SubmitEventArgs(string text)
            : base()
        {
            Text = text;
        }
    }

    public partial class AddTaskControl : UserControl, INotifyPropertyChanged
    {
        #region Top Property

        public static readonly DependencyProperty TopProperty =
            DependencyProperty.Register("TopProperty", typeof(double), typeof(AddTaskControl), new PropertyMetadata(0d));
        
        public double Top
        {
            get { return (double)GetValue(TopProperty); }
            set 
            { 
                SetValue(TopProperty, value);
                OnPropertyChanged("MainGridMargin");
            }
        }

        public Thickness MainGridMargin
        {
            get
            {
                return new Thickness(0, Top, 0, 0);
            }
        }

        #endregion

        #region IsOpen Property

        private bool _isOpen = false;
        public bool IsOpen
        {
            get
            {
                return _isOpen;
            }
            set
            {
                if (value != _isOpen)
                {
                    if (value)
                    {
                        Open();
                    }
                    else
                    {
                        Close();
                    }
                }
            }
        }

        #endregion

        #region Construction

        public AddTaskControl()
        {
            InitializeComponent();
        }

        #endregion

        #region Open and Close Events

        public void Open()
        {
            Visibility = System.Windows.Visibility.Visible;
            _isOpen = true;
            OnPropertyChanged("IsOpen");
            SwivelIn.Begin();
        }

        public void Close()
        {
            SwivelOut.Begin();
            
        }

        private void Overlay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SwivelIn.Completed += (s, ev) =>
            {
                SmartAddBox.Focus();
            };

            SwivelOut.Completed += (s, ev) =>
            {
                Visibility = System.Windows.Visibility.Collapsed;
                _isOpen = false;
                SmartAddBox.Text = "";
                OnPropertyChanged("IsOpen");
            };
        }

        #endregion

        #region Submit Event

        public event SubmitEventHandler Submit;
        public void DoSubmit()
        {
            Close();

            if (Submit != null)
            {
                Submit(this, new SubmitEventArgs(SmartAddBox.Text));
            }
        }

        #endregion

        #region Submit Triggers

        private void SmartAddBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DoSubmit();
            }
        }

        private void SmartAddButton_Click(object sender, EventArgs e)
        {
            if (Submit != null)
            {
                DoSubmit();
            }
        }

        #endregion

        #region Hyperlink Button Event Handlers

        private void AdvancedAddButton_Click(object sender, RoutedEventArgs e)
        {
            PhoneApplicationFrame frame = App.Current.RootVisual as PhoneApplicationFrame;
            frame.Navigate(new Uri("/Gui/AddTaskPage.xaml", UriKind.Relative));

            Close();
        }

        private void CancelSmartAddButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion

        #region INofityPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
