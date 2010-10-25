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

namespace WinMilk.Gui.Controls
{
    public partial class AddTaskControl : UserControl, INotifyPropertyChanged
    {
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

        public AddTaskControl()
        {
            InitializeComponent();
        }

        public void Open()
        {
            SmartAddPopup.IsOpen = true;
            Overlay.Visibility = System.Windows.Visibility.Visible;
            _isOpen = true;
            OnPropertyChanged("IsOpen");
            SwivelIn.Begin();
        }

        public void Close()
        {
            SwivelOut.Begin();
            
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName);
            }
        }

        private void Overlay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SwivelIn.Completed += (s, ev) =>
            {
                
            };

            SwivelOut.Completed += (s, ev) =>
            {
                SmartAddPopup.IsOpen = false;
                Overlay.Visibility = System.Windows.Visibility.Collapsed;
                _isOpen = false;
                OnPropertyChanged("IsOpen");
            };

            
        }
    }
}
