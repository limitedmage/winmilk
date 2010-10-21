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
using IronCow.Rest;

namespace WinMilk.Rtm
{
    [DataContract]
    public class NoteViewModel : ElementViewModel
    {
        #region Fields

        private string mTitle;
        private string mBody;

        private TaskViewModel mParentTask;

        #endregion

        #region Properties

        [DataMember]
        public string Title
        {
            get
            {
                return mTitle;
            }
            set
            {
                mTitle = value;
                OnPropertyChanged("Title");
            }
        }

        [DataMember]
        public string Body
        {
            get
            {
                return mBody;
            }
            set
            {
                mBody = value;
                OnPropertyChanged("Body");
            }
        }

        [DataMember]
        public TaskViewModel ParentTask
        {
            get
            {
                return mParentTask;
            }
            set
            {
                mParentTask = value;
                OnPropertyChanged("ParentTask");
            }
        }

        #endregion

        #region Constructors

        public NoteViewModel()
            : base()
        {
        }

        public NoteViewModel(TaskViewModel parentTask, RawNote note)
        {
            ParentTask = parentTask;
            Title = note.Title;
            Body = note.Body;
        }

        #endregion
    }
}
