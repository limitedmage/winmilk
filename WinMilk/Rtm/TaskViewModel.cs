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

namespace WinMilk.Rtm
{
    public enum TaskPriority
    {
        None = 0,
        One = 1,
        Two = 2,
        Three = 3
    }

    [DataContract]
    public class TaskViewModel : ElementViewModel
    {
        #region Members

        private int mSeriedId;
        private DateTime mCreated;
        private DateTime mAdded;
        private DateTime? mModified;
        private DateTime? mCompleted;
        private DateTime? mDeleted;

        private DateTime? mDue;
        private bool mHasDueTime;
        private int mPostponed;

        private TaskPriority mPriority;

        private ListViewModel mParentList;

        private ObservableCollection<string> mTags;
        private ObservableCollection<NoteViewModel> mNotes;

        #endregion

        #region Fields

        [DataMember]
        public int SeriesId
        {
            get
            {
                return mSeriedId;
            }
            set
            {
                mSeriedId = value;
                OnPropertyChanged("SeriesId");
            }
        }

        [DataMember]
        public DateTime Created
        {
            get
            {
                return mCreated;
            }
            set
            {
                mCreated = value;
                OnPropertyChanged("Created");
            }
        }

        [DataMember]
        public DateTime Added
        {
            get
            {
                return mAdded;
            }
            set
            {
                mAdded = value;
                OnPropertyChanged("Added");
            }
        }

        [DataMember]
        public DateTime? Modified
        {
            get
            {
                return mModified;
            }
            set
            {
                mModified = value;
                OnPropertyChanged("Modified");
            }
        }

        [DataMember]
        public DateTime? Completed
        {
            get
            {
                return mCompleted;
            }
            set
            {
                mCompleted = value;
                OnPropertyChanged("Completed");
                OnPropertyChanged("IsCompleted");
                OnPropertyChanged("IsIncomplete");
            }
        }

        [DataMember]
        public DateTime? Deleted
        {
            get
            {
                return mDeleted;
            }
            set
            {
                mDeleted = value;
                OnPropertyChanged("Deleted");
                OnPropertyChanged("IsDeleted");
            }
        }

        [DataMember]
        public DateTime? Due
        {
            get
            {
                return mDue;
            }
            set
            {
                mDue = value;
                OnPropertyChanged("Due");
                OnPropertyChanged("IsLate");
            }
        }

        [DataMember]
        public bool HasDueTime
        {
            get
            {
                return mHasDueTime;
            }
            set
            {
                mHasDueTime = value;
                OnPropertyChanged("HasDueTime");
            }
        }

        [DataMember]
        public bool IsLate
        {
            get
            {
                if (!HasDueTime)
                {
                    return Due < DateTime.Today;
                }
                else
                {
                    return Due < DateTime.Now;
                }
            }
        }

        [DataMember]
        public int Postponed
        {
            get
            {
                return mPostponed;
            }
            set
            {
                mPostponed = value;
                OnPropertyChanged("Postponed");
            }
        }

        [DataMember]
        public TaskPriority Priority
        {
            get
            {
                return mPriority;
            }
            set
            {
                mPriority = value;
                OnPropertyChanged("Priority");
            }
        }

        [DataMember]
        public ListViewModel ParentList
        {
            get
            {
                return mParentList;
            }
            set
            {
                mParentList = value;
                OnPropertyChanged("ParentList");
            }
        }

        [DataMember]
        public ObservableCollection<string> Tags
        {
            get
            {
                return mTags;
            }
            set
            {
                mTags = value;
                OnPropertyChanged("Tags");
            }
        }

        [DataMember]
        public ObservableCollection<NoteViewModel> Notes
        {
            get
            {
                return mNotes;
            }
            set
            {
                mNotes = value;
                OnPropertyChanged("Notes");
            }
        }


        public bool IsCompleted { get { return Completed.HasValue; } }
        public bool IsIncomplete { get { return !Completed.HasValue; } }
        public bool IsDeleted { get { return Deleted.HasValue; } }

        #endregion

        #region Constructors

        public TaskViewModel()
            : base()
        {
        }

        public TaskViewModel(ListViewModel parentList, RawTaskSeries taskSeries, RawTask task)
            : base(task as RawRtmElement)
        {
            SeriesId = taskSeries.Id;
            Created = DateTime.Parse(taskSeries.Created);
            Added = DateTime.Parse(task.Added);

            if (string.IsNullOrEmpty(taskSeries.Modified))
            {
                Modified = null;
            }
            else
            {
                Modified = DateTime.Parse(taskSeries.Modified);
            }

            if (string.IsNullOrEmpty(task.Completed))
            {
                Completed = null;
            }
            else
            {
                Completed = DateTime.Parse(task.Completed);
            }

            if (string.IsNullOrEmpty(task.Deleted))
            {
                Deleted = null;
            }
            else
            {
                Deleted = DateTime.Parse(task.Deleted);
            }

            HasDueTime = task.HasDueTime == 1 ? true : false;

            if (string.IsNullOrEmpty(task.Postponed))
            {
                Postponed = 0;
            }
            else
            {
                Postponed = int.Parse(task.Postponed);
            }


            if (string.IsNullOrEmpty(task.Priority) || task.Priority == "N")
            {
                Priority = TaskPriority.None;
            }
            else
            {
                Priority = (TaskPriority)int.Parse(task.Priority);
            }

            ParentList = parentList;

            Tags = new ObservableCollection<TagViewModel>();
            foreach (string s in taskSeries.Tags)
            {
                Tags.Add(new TagViewModel(this, s));
            }

            Notes = new ObservableCollection<NoteViewModel>();
            foreach (RawNote n in taskSeries.Notes)
            {
                Notes.Add(new NoteViewModel(this, n));
            }
        }

        #endregion
    }
}
