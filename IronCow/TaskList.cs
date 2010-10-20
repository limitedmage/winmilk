using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using IronCow.Rest;
using System.ComponentModel;

namespace IronCow
{
    public class TaskList : RtmFatElement, INotifyPropertyChanged
    {
        #region Callback Delegates

        public delegate void VoidCallback();

        #endregion

        #region Public Properties
        public int Position { get; private set; }
        public string Filter { get; private set; }
        public TaskListFlags Flags { get; private set; }

        private bool mIsFrozen = false;
        public bool IsFrozen
        {
            get { return mIsFrozen; }
            set
            {
                if (!GetFlag(TaskListFlags.Smart))
                    throw new InvalidOperationException("Only smart lists can be frozen.");
                mIsFrozen = true;
            }
        }

        private string mName;
        public string Name
        {
            get { return mName; }
            set
            {
                if (mName != value)
                {
                    mName = value;

                    if (Syncing && IsSynced)
                    {
                        RestRequest request = new RestRequest("rtm.lists.setName");
                        request.Parameters.Add("timeline", Owner.GetTimeline().ToString());
                        request.Parameters.Add("list_id", Id.ToString());
                        request.Parameters.Add("name", value);
                        Owner.ExecuteRequest(request);
                    }

                    OnPropertyChanged("Name");
                }
            }
        }

        private TaskListTaskCollection mTasks;
        public TaskListTaskCollection Tasks
        {
            get
            {
                if (mTasks == null)
                {
                    lock (this)
                    {
                        if (mTasks == null)
                        {
                            TaskListTaskCollection tmp = new TaskListTaskCollection(this);
                            tmp.Resync();
                            System.Threading.Interlocked.Exchange(ref mTasks, tmp);
                        }
                    }
                }
                else if (!IsFrozen && GetFlag(TaskListFlags.Smart))
                {
                    // Resync all the time for smart lists...
                    //TODO: maybe use cache like normal lists, but for only a short time?
                    lock (this)
                    {
                        mTasks.SmartResync();
                    }
                }
                return mTasks;
            }
        } 
        #endregion

        #region Construction
        public TaskList()
        {
        }

        public TaskList(string name)
        {
            mName = name;
        }

        internal TaskList(RawList list)
        {
            Sync(list);
        } 
        #endregion

        #region Public Methods
        public void Archive(VoidCallback callback)
        {
            if (Syncing)
            {
                if (!GetFlag(TaskListFlags.Archived))
                {
                    RestRequest request = new RestRequest("rtm.lists.archive");
                    request.Parameters.Add("timeline", Owner.GetTimeline().ToString());
                    request.Parameters.Add("list_id", Id.ToString());
                    request.Callback = (response) => { callback(); };
                    Owner.ExecuteRequest(request);
                }
            }

            SetFlag(TaskListFlags.Archived, true);
        }

        public void Unarchive(VoidCallback callback)
        {
            if (Syncing)
            {
                if (GetFlag(TaskListFlags.Archived))
                {
                    RestRequest request = new RestRequest("rtm.lists.unarchive");
                    request.Parameters.Add("timeline", Owner.GetTimeline().ToString());
                    request.Parameters.Add("list_id", Id.ToString());
                    request.Callback = (response) => { callback(); };
                    Owner.ExecuteRequest(request);

                    SetFlag(TaskListFlags.Archived, false);
                }
            }
            
            SetFlag(TaskListFlags.Archived, false);
        }

        public void AddTask(Task task, VoidCallback callback)
        {
            // Freeze this task list so that we don't needlessly resync a smart-list
            // while adding a task to it.
            bool previousFreezeState = mIsFrozen;
            mIsFrozen = true;
            try
            {
                Tasks.Add(task);
            }
            finally
            {
                mIsFrozen = previousFreezeState;
            }
        }

        public bool GetFlag(TaskListFlags flag)
        {
            return (Flags & flag) == flag;
        } 
        #endregion

        #region Syncing
        internal void InternalSync(RawList rawList)
        {
            lock (this)
            {
                if (mTasks == null)
                {
                    TaskListTaskCollection tmp = new TaskListTaskCollection(this);
                    tmp.InternalSync(new RawList[] { rawList });
                    System.Threading.Interlocked.Exchange(ref mTasks, tmp);
                }
                else
                {
                    mTasks.InternalSync(new RawList[] { rawList });
                }
            }
        }

        protected override void DoSync(bool firstSync, RawRtmElement element)
        {
            base.DoSync(firstSync, element);

            RawList list = (RawList)element;
            mName = list.Name;
            Filter = list.Filter;
            Position = list.Position;
            SetFlag(TaskListFlags.Archived, list.Archived == 1);
            SetFlag(TaskListFlags.Deleted, list.Deleted == 1);
            SetFlag(TaskListFlags.Locked, list.Locked == 1);
            SetFlag(TaskListFlags.Smart, list.Smart == 1);
        } 
        #endregion

        #region Private Methods
        private void SetFlag(TaskListFlags flag, bool value)
        {
            if (value)
            {
                Flags |= flag;
            }
            else
            {
                Flags &= ~flag;
            }
        }
        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                if (Rtm.Dispatcher != null && !Rtm.Dispatcher.CheckAccess())
                    Rtm.Dispatcher.BeginInvoke(new Action(() => PropertyChanged(this, new PropertyChangedEventArgs(propertyName))));
                else
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
