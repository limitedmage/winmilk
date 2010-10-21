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
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using IronCow.Rest;
using IronCow.Search;

namespace WinMilk.Rtm
{
    [DataContract]
    public class ListViewModel : ElementViewModel
    {
        #region Fields

        private string mName;

        private bool mIsSmart;
        private string mFilter;

        private int mPosition;

        private ObservableCollection<TaskViewModel> mTasks;

        #endregion

        #region Properties

        [DataMember]
        public string Name
        {
            get
            {
                return mName;
            }
            set
            {
                mName = value;
                OnPropertyChanged("Name");
            }
        }

        [DataMember]
        public bool IsSmart
        {
            get
            {
                return mIsSmart;
            }
            set
            {
                mIsSmart = value;
                OnPropertyChanged("IsSmart");
                OnPropertyChanged("IsNormal");
            }
        }

        public bool IsNormal
        {
            get
            {
                return !IsSmart;
            }
        }

        [DataMember]
        public string Filter
        {
            get
            {
                return mFilter;
            }
            set
            {
                mFilter = value;
                OnPropertyChanged("Filter");
            }
        }

        [DataMember]
        public ObservableCollection<TaskViewModel> Tasks
        {
            get
            {
                return mTasks;
            }
            set
            {
                mTasks = value;
                OnPropertyChanged("Tasks");
            }
        }

        #endregion

        #region Constructors

        public ListViewModel()
            : base()
        {
        }

        public ListViewModel(RawList list)
        {
            Name = list.Name;
            Filter = list.Filter;
            IsSmart = list.Smart == 1;

            Tasks = new ObservableCollection<TaskViewModel>();
        }

        public void PopulateTasks(ObservableCollection<TaskViewModel> tasks)
        {
            Tasks = new ObservableCollection<TaskViewModel>();

            if (IsNormal)
            {
                foreach (TaskViewModel t in tasks)
                {
                    if (t.ParentList == this)
                    {
                        Tasks.Add(t);
                    }
                }
            }
            else
            {
                
            }
        }

        #endregion
    }
}
