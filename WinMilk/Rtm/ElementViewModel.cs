using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Runtime.Serialization;
using System.ComponentModel;
using IronCow.Rest;

namespace WinMilk.Rtm
{
    [DataContract]
    public class ElementViewModel : INotifyPropertyChanged
    {
        #region Fields

        private int mId;

        #endregion

        #region Properties

        [DataMember]
        public int Id
        {
            get
            {
                return mId;
            }
            set
            {
                mId = value;
                OnPropertyChanged("Id");
            }
        }

        #endregion

        #region Contructors

        public ElementViewModel(RawRtmElement rawElement)
        {
            Id = rawElement.Id;
        }

        public ElementViewModel()
        {
            Id = 0;
        }

        #endregion

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName))
            }
        }

        #endregion
    }
}
