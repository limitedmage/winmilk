using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using IronCow.Rest;
using System.Globalization;
using System.Collections.Specialized;
using System.Windows.Media;

namespace IronCow
{
    public class Task : RtmFatElement, INotifyPropertyChanged, IComparable
    {
        #region Callback Delegates

        public delegate void VoidCallback();

        #endregion

        #region Sync Requests
        private static RestRequest CreateStandardRequest(Task task, string method, VoidCallback callback)
        {
            RestRequest request = new RestRequest(method);
            request.Parameters.Add("timeline", task.Owner.GetTimeline().ToString());
            request.Parameters.Add("list_id", task.Parent.Id.ToString());
            request.Parameters.Add("taskseries_id", task.SeriesId.ToString());
            request.Parameters.Add("task_id", task.Id.ToString());
            request.Callback = (response) => { callback(); };
            return request;
        }

        private static RestRequest CreateSetUrlRequest(Task task, string url, VoidCallback callback)
        {
            RestRequest request = CreateStandardRequest(task, "rtm.tasks.setURL", callback);
            if (url != null)
                request.Parameters.Add("url", url);
            return request;
        }

        private static RestRequest CreateSetLocationRequest(Task task, string locationId, VoidCallback callback)
        {
            RestRequest request = CreateStandardRequest(task, "rtm.tasks.setLocation", callback);
            if (locationId != null)
                request.Parameters.Add("location_id", locationId);
            return request;
        }

        private static RestRequest CreateSetDueDateRequest(Task task, string due, VoidCallback callback)
        {
            RestRequest request = CreateStandardRequest(task, "rtm.tasks.setDueDate", callback);
            if (due != null)
            {
                request.Parameters.Add("due", due);
                request.Parameters.Add("parse", "1");
            }
            return request;
        }

        private static RestRequest CreateSetPriorityRequest(Task task, string priority, VoidCallback callback)
        {
            RestRequest request = CreateStandardRequest(task, "rtm.tasks.setPriority", callback);
            if (priority != null)
                request.Parameters.Add("priority", priority);
            return request;
        }

        private static string TaskPriorityToPriorityRequestParameter(TaskPriority priority)
        {
            switch (priority)
            {
                case TaskPriority.One:
                    return "1";
                case TaskPriority.Two:
                    return "2";
                case TaskPriority.Three:
                    return "3";
                case TaskPriority.None:
                default:
                    return "";
            }
        }

        private static RestRequest CreateSetRecurrenceRequest(Task task, string recurrence, VoidCallback callback)
        {
            RestRequest request = CreateStandardRequest(task, "rtm.tasks.setRecurrence", callback);
            if (recurrence != null)
                request.Parameters.Add("repeat", recurrence);
            return request;
        }

        private static RestRequest CreateSetEstimateRequest(Task task, string estimate, VoidCallback callback)
        {
            RestRequest request = CreateStandardRequest(task, "rtm.tasks.setEstimate", callback);
            if (estimate != null)
                request.Parameters.Add("estimate", estimate);
            return request;
        }
        #endregion

        #region Public Properties
        public int SeriesId { get; private set; }
        public string Source { get; private set; }
        public DateTime Created { get; private set; }
        public DateTime Added { get; private set; }
        public DateTime? Modified { get; private set; }
        public DateTime? Completed { get; private set; }
        public DateTime? Deleted { get; private set; }
        public bool HasDueTime { get; private set; }
        public bool IsLate { get; private set; }
        public int Postponed { get; private set; }
        public TaskTagCollection Tags { get; private set; }
        public TaskTaskNoteCollection Notes { get; private set; }

        public bool IsCompleted { get { return Completed.HasValue; } }
        public bool IsIncomplete { get { return !Completed.HasValue; } }
        public bool IsDeleted { get { return Deleted.HasValue; } }

        public string TagsString
        {
            get
            {
                if (Tags != null)
                {
                    return string.Join(", ", Tags.ToArray());
                }
                return "";
            }
        }

        public bool HasTags
        {
            get
            {
                if (Tags != null && Tags.Count > 0)
                {
                    return true;
                }
                return false;
            }
        }

        private TaskList mParent;
        public TaskList Parent
        {
            get { return mParent; }
            set
            {
                if (mParent != value)
                {
                    if (value != null)
                        value.Tasks.Add(this);
                    else
                        mParent.Tasks.Remove(this);
                }

                OnPropertyChanged("List");
            }
        }

        public string List
        {
            get
            {
                return Parent.Name;
            }
        }

        internal void SetParentInternal(TaskList parent)
        {
            mParent = parent;
            if (mParent == null)
                Owner = null;
            else
                Owner = mParent.Owner;
            OnPropertyChanged("Parent");
            OnPropertyChanged("ParentName");
        }

        public string ParentName
        {
            get
            {
                if (Parent == null)
                    return null;
                return Parent.Name;
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
                        RestRequest request = CreateStandardRequest(this, "rtm.tasks.setName", () => { });
                        request.Parameters.Add("name", value);
                        Owner.ExecuteRequest(request);
                    }

                    OnPropertyChanged("Name");
                }
            }
        }

        private string mUrl;
        public string Url
        {
            get { return mUrl; }
            set
            {
                if (mUrl != value)
                {
                    mUrl = value;

                    if (Syncing)
                    {
                        if (IsSynced)
                        {
                            RestRequest request = CreateSetUrlRequest(this, mUrl, () => { });
                            Owner.ExecuteRequest(request);
                        }
                    }

                    OnPropertyChanged("Url");
                }
            }
        }

        private int mLocationId = RtmElement.UnsyncedId;
        private Location mLocation;
        public Location Location
        {
            get
            {
                if (mLocation == null && mLocationId != RtmElement.UnsyncedId)
                {
                    if (!IsSynced)
                        throw new IronCowException("This task has a valid location ID but is not synced... impossible!");
                    foreach (var location in Owner.Locations)
                    {
                        if (location.Id == mLocationId)
                        {
                            mLocation = location;
                            break;
                        }
                    }
                    if (mLocation == null)
                        throw new IronCowException(string.Format("Can't find location with ID '{0}'.", mLocationId));
                }
                return mLocation;
            }
            set
            {
                if (mLocation != value)
                {
                    if (mLocation != null && !mLocation.IsSynced)
                    {
                        // The location was set to a new unsynced location, but is now changed
                        // to a new location before the first one had a chance to be synced. We
                        // therefore have to remove our location-first-sync handler.
                        mLocation.Synced -= new EventHandler(SetLocationWhenLocationFirstSynced);
                    }

                    mLocation = value;

                    if (Syncing)
                    {
                        if (IsSynced)
                        {
                            if (mLocation == null || mLocation.IsSynced)
                            {
                                mLocationId = mLocation == null ? RtmElement.UnsyncedId : mLocation.Id;
                                RestRequest request = CreateSetLocationRequest(this, mLocationId == RtmElement.UnsyncedId ? null : mLocationId.ToString(), () => { });
                                Owner.ExecuteRequest(request);
                            }
                            else
                            {
                                // The location is not synced, so we have to wait for it to be synced
                                // in order to have an ID for it. We'll listen to its Sync event.
                                mLocation.Synced += new EventHandler(SetLocationWhenLocationFirstSynced);
                            }
                        }
                    }

                    OnPropertyChanged("Location");
                    OnPropertyChanged("LocationName");
                }
            }
        }

        private void SetLocationWhenLocationFirstSynced(object sender, EventArgs e)
        {
            mLocationId = mLocation.Id;
            System.Diagnostics.Debug.Assert(mLocationId != RtmElement.UnsyncedId);
            mLocation.Synced -= new EventHandler(SetLocationWhenLocationFirstSynced);

            RestRequest request = CreateSetLocationRequest(this, mLocationId.ToString(), () => { });
            Owner.ExecuteRequest(request);
        }

        public string LocationName
        {
            get
            {
                if (Location == null)
                    return null;
                return Location.Name;
            }
        }

        private string mDue;
        public string Due
        {
            get { return mDue; }
            set
            {
                if (mDue != value)
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        HasDueTime = false;
                        mDueDateTime = null;
                        mDue = null;
                    }
                    else
                    {
                        var dateFormat = IsSynced ? Owner.UserSettings.DateFormat : DateFormat.Default;
                        var timeFormat = IsSynced ? Owner.UserSettings.TimeFormat : TimeFormat.Default;
                        var dateTime = DateConverter.ParseDateTime(value, dateFormat);

                        HasDueTime = dateTime.HasTime;
                        mDueDateTime = dateTime.DateTime;
                        mDue = DateConverter.FormatDateTime(dateTime, dateFormat, timeFormat);
                    }

                    if (Syncing)
                    {
                        if (IsSynced)
                        {
                            RestRequest request = CreateSetDueDateRequest(this, mDue, () => { });
                            Owner.ExecuteRequest(request);
                        }
                    }

                    OnPropertyChanged("Due");
                    OnPropertyChanged("DueDateTime");
                }
            }
        }

        public string DueString
        {
            get
            {
                string dueString = "";
                if (DueDateTime.HasValue)
                {
                    if (DueDateTime.Value.Date == DateTime.Today)
                    {
                        dueString += "Today";
                    }
                    else if (DateTime.Today.AddDays(1) == DueDateTime.Value.Date)
                    {
                        dueString += "Tomorrow";
                    }
                    else if (DateTime.Today < DueDateTime.Value.Date && DateTime.Today.AddDays(6) >= DueDateTime.Value.Date)
                    {
                        dueString += DueDateTime.Value.ToString("dddd");
                    }
                    else
                    {
                        dueString += DueDateTime.Value.ToString("ddd d MMM");
                    }

                    if (this.HasDueTime)
                    {
                        dueString += " " + DueDateTime.Value.ToString("t");
                    }
                }

                return dueString;
            }
        }

        public string LongDueDateString
        {
            get
            {
                string dueString = "Never";
                if (DueDateTime.HasValue)
                {
                    if (this.HasDueTime)
                    {
                        dueString = DueDateTime.Value.ToString("f");
                    }
                    else
                    {
                        dueString = DueDateTime.Value.ToString("D");
                    }
                }

                return dueString;
            }
        }

        private DateTime? mDueDateTime;
        public DateTime? DueDateTime
        {
            get
            {
                return mDueDateTime;
            }
            set
            {
                if (value == null)
                {
                    Due = null;
                }
                else
                {
                    Due = value.Value.ToString("s");
                }
            }
        }

        public FuzzyDateTime FuzzyDueDateTime
        {
            get
            {
                if (!DueDateTime.HasValue)
                    throw new InvalidOperationException("There is no due date time on this task.");
                return new FuzzyDateTime(DueDateTime.Value, HasDueTime);
            }
            set
            {
                if (value.HasTime)
                    DueDateTime = value.DateTime;
                else
                    Due = value.DateTime.ToString("yyyy-MM-dd");
            }
        }

        private TaskPriority mPriority;
        public TaskPriority Priority
        {
            get { return mPriority; }
            set
            {
                if (mPriority != value)
                {
                    mPriority = value;

                    if (Syncing)
                    {
                        if (IsSynced)
                        {
                            RestRequest request = CreateSetPriorityRequest(this, TaskPriorityToPriorityRequestParameter(mPriority), () => { });
                            Owner.ExecuteRequest(request);
                        }
                    }

                    OnPropertyChanged("Priority");
                    OnPropertyChanged("Importance");
                    OnPropertyChanged("PriorityColor");
                }
            }
        }

        public string PriorityColor
        {
            get
            {
                switch (Priority)
                {
                    case TaskPriority.One:
                        return "#EA5200";
                    case TaskPriority.Two:
                        return "#0060BF";
                    case TaskPriority.Three:
                        return "#359AFF";
                    default:
                        return Colors.Transparent.ToString();
                }
            }
        }

        public int Importance
        {
            get
            {
                switch (Priority)
                {
                    case TaskPriority.One:
                        return 0;
                    case TaskPriority.Two:
                        return 1;
                    case TaskPriority.Three:
                        return 2;
                    case TaskPriority.None:
                    default:
                        return 3;
                }
            }
        }

        private string mRecurrence;
        public string Recurrence
        {
            get { return mRecurrence; }
            set
            {
                if (mRecurrence != value)
                {
                    mRecurrence = value;

                    if (Syncing)
                    {
                        if (IsSynced)
                        {
                            RestRequest request = CreateSetRecurrenceRequest(this, mRecurrence, () => { });
                            Owner.ExecuteRequest(request);
                        }
                    }

                    OnPropertyChanged("Recurrence");
                }
            }
        }

        public bool HasRecurrence
        {
            get { return !string.IsNullOrEmpty(Recurrence); }
        }

        private string mEstimate;
        public string Estimate
        {
            get { return mEstimate; }
            set
            {
                if (mEstimate != value)
                {
                    mEstimate = value;

                    if (Syncing)
                    {
                        if (IsSynced)
                        {
                            RestRequest request = CreateSetEstimateRequest(this, mEstimate, () => { });
                            Owner.ExecuteRequest(request);
                        }
                    }

                    OnPropertyChanged("Estimate");
                }
            }
        }
        #endregion

        #region Construction
        public Task()
        {
            Syncing = true;
            Tags = new TaskTagCollection(this);
            Notes = new TaskTaskNoteCollection(this);
        }

        public Task(string name)
            : this()
        {
            mName = name;
        }
        #endregion

        #region Public Methods
        public void Complete(VoidCallback callback)
        {
            if (Syncing)
            {
                if (!IsSynced)
                    throw new InvalidOperationException("Can't complete a task that has not been synced.");

                Request request = CreateStandardRequest(this, "rtm.tasks.complete", callback);
                
                Owner.ExecuteRequest(request);
            }
            
            Completed = DateTime.Now;
            OnPropertyChanged("Completed");
            OnPropertyChanged("IsCompleted");
            OnPropertyChanged("IsIncomplete");
        }

        public void Uncomplete(VoidCallback callback)
        {
            if (Syncing)
            {
                if (!IsSynced)
                    throw new InvalidOperationException("Can't uncomplete a task that has not been synced.");

                Request request = CreateStandardRequest(this, "rtm.tasks.uncomplete", callback);
                Owner.ExecuteRequest(request);
            }

            Completed = null;
            OnPropertyChanged("Completed");
            OnPropertyChanged("IsCompleted");
            OnPropertyChanged("IsIncomplete");
        }

        public void Postpone(VoidCallback callback)
        {
            if (Syncing)
            {
                if (!IsSynced)
                    throw new InvalidOperationException("Can't postpone a task that has not been synced.");

                Request request = CreateStandardRequest(this, "rtm.tasks.postpone", callback);
                Owner.ExecuteRequest(request);
            }

            Postponed++;
            OnPropertyChanged("Postponed");
        }

        public void Delete(VoidCallback callback)
        {
            if (Syncing)
            {
                if (!IsSynced)
                    throw new InvalidOperationException("Can't delete a task that has not been synced.");

                Request request = CreateStandardRequest(this, "rtm.tasks.delete", callback);
                Owner.ExecuteRequest(request);
            }
            
            Deleted = DateTime.Now;
            OnPropertyChanged("Deleted");
        }

        public void SetTags(string formattedTags, char[] separators)
        {
            string[] tags = formattedTags.Split(separators, StringSplitOptions.RemoveEmptyEntries);           
            SetTags(tags);
        }

        public void SetTags(IEnumerable<string> tags)
        {
            Tags.SetTags(tags);
        }

        public string GetTags(string separator)
        {
            StringBuilder tagsBuilder = new StringBuilder();
            foreach (var tag in Tags)
            {
                if (tagsBuilder.Length > 0)
                    tagsBuilder.Append(separator);
                tagsBuilder.Append(tag);
            }
            return tagsBuilder.ToString();
        }
        #endregion

        #region Sync Methods
        protected override void OnOwnerChanged()
        {
            // Update the user-friendly version of the due date depending on
            // the UserSettings (accessible only from Rtm).
            if (DueDateTime.HasValue)
                SetDueAndIsLate(DueDateTime, HasDueTime);
            // Notify our notes.
            Notes.OnOwnerChanged();
            base.OnOwnerChanged();
        }

        protected override void OnSyncingChanged()
        {
            Tags.Syncing = Syncing;
            Notes.Syncing = Syncing;
            base.OnSyncingChanged();
        }

        protected override void DoSync(bool firstSync, RawRtmElement element)
        {
            base.DoSync(firstSync, element);

            TaskBundle bundle = (TaskBundle)element;
            SeriesId = bundle.Series.Id;
            switch (bundle.SyncMode)
            {
                case TaskSyncMode.Download:
                    DoDownloadSync(firstSync, bundle);
                    break;
                case TaskSyncMode.Upload:
                    DoUploadSync(firstSync, bundle);
                    break;
                default:
                    throw new NotImplementedException("Internal Error: unsupported TaskSyncMode.");
            }
        }

        private void DoDownloadSync(bool firstSync, TaskBundle bundle)
        {
            // Retrieve all our values from the server.
            RawTaskSeries series = bundle.Series;
            DoDownloadSyncFromTaskSeries(firstSync, series);

            RawTask task = bundle.Task;
            DoDownloadSyncFromTask(firstSync, task);

            // Notify of property changes.
            OnPropertyChanged("Added");
            OnPropertyChanged("Completed");
            OnPropertyChanged("Created");
            OnPropertyChanged("Deleted");
            OnPropertyChanged("Due");
            OnPropertyChanged("DueDateTime");
            OnPropertyChanged("Estimate");
            OnPropertyChanged("HasDueTime");
            OnPropertyChanged("IsCompleted");
            OnPropertyChanged("IsDeleted");
            OnPropertyChanged("IsLate");
            OnPropertyChanged("IsIncomplete");
            OnPropertyChanged("Location");
            OnPropertyChanged("Modified");
            OnPropertyChanged("Name");
            OnPropertyChanged("Notes");
            OnPropertyChanged("Parent");
            OnPropertyChanged("ParentName");
            OnPropertyChanged("Postponed");
            OnPropertyChanged("Priority");
            OnPropertyChanged("Recurrence");
            OnPropertyChanged("SeriesId");
            OnPropertyChanged("Source");
            OnPropertyChanged("Tags");
            OnPropertyChanged("Url");
        }

        private void DoDownloadSyncFromTaskSeries(bool firstSync, RawTaskSeries series)
        {
            Source = series.Source;

            mName = series.Name;
            mUrl = series.Url;

            Created = DateTime.Parse(series.Created);
            mLocationId = RtmElement.UnsyncedId;
            if (!string.IsNullOrEmpty(series.LocationId))
                mLocationId = int.Parse(series.LocationId);
            if (!string.IsNullOrEmpty(series.Modified))
                Modified = DateTime.Parse(series.Modified);
            else
                Modified = null;

            SetRecurrence(series.RepeatRule);

            DownloadSyncTags(firstSync, series);
            DownloadSyncNotes(firstSync, series);
        }

        private void DoDownloadSyncFromTask(bool firstSync, RawTask task)
        {
            if (string.IsNullOrEmpty(task.Due))
            {
                SetDueAndIsLate(null, false);
            }
            else
            {
                DateTime dueDateTime = DateTime.Parse(task.Due);
                SetDueAndIsLate(dueDateTime, task.HasDueTime == 1);
            }

            mEstimate = task.Estimate;
            Added = DateTime.Parse(task.Added);

            if (!string.IsNullOrEmpty(task.Completed))
                Completed = DateTime.Parse(task.Completed);
            else
                Completed = null;

            if (!string.IsNullOrEmpty(task.Deleted))
                Deleted = DateTime.Parse(task.Deleted);
            else
                Deleted = null;

            if (string.IsNullOrEmpty(task.Priority) || task.Priority == "N")
                mPriority = TaskPriority.None;
            else
                mPriority = (TaskPriority)int.Parse(task.Priority);

            if (!string.IsNullOrEmpty(task.Postponed))
                Postponed = int.Parse(task.Postponed);
            else
                Postponed = 0;
        }

        private void DownloadSyncTags(bool firstSync, RawTaskSeries series)
        {
            Tags.UnsyncedClear();
            if (series.Tags != null)
            {
                foreach (var tag in series.Tags)
                {
                    Tags.UnsyncedAdd(tag);
                }
            }
        }

        private void DownloadSyncNotes(bool firstSync, RawTaskSeries series)
        {
            Notes.UnsyncedClear();
            if (series.Notes != null)
            {
                foreach (var note in series.Notes)
                {
                    Notes.UnsyncedAdd(new TaskNote(note));
                }
            }
        }

        private void DoUploadSync(bool firstSync, TaskBundle bundle)
        {
            // Upload all our values to the server.
            if (!string.IsNullOrEmpty(mDue))
            {
                RestRequest request = CreateSetDueDateRequest(this, mDue, () => { });
                Owner.ExecuteRequest(request);
            }
            if (!string.IsNullOrEmpty(mEstimate))
            {
                RestRequest request = CreateSetEstimateRequest(this, mEstimate, () => { });
                Owner.ExecuteRequest(request);
            }
            if (mLocation != null)
            {
                if (mLocation.IsSynced)
                {
                    RestRequest request = CreateSetLocationRequest(this, mLocation.Id.ToString(), () => { });
                    Owner.ExecuteRequest(request);
                }
            }
            if (mPriority != TaskPriority.None)
            {
                RestRequest request = CreateSetPriorityRequest(this, TaskPriorityToPriorityRequestParameter(mPriority), () => { });
                Owner.ExecuteRequest(request);
            }
            if (!string.IsNullOrEmpty(mRecurrence))
            {
                RestRequest request = CreateSetRecurrenceRequest(this, mRecurrence, () => { });
                Owner.ExecuteRequest(request);
            }
            if (!string.IsNullOrEmpty(mUrl))
            {
                RestRequest request = CreateSetUrlRequest(this, mUrl, () => { });
                Owner.ExecuteRequest(request);
            }
            if (Tags.Count > 0)
            {
                Tags.UploadTags();
            }
            if (Notes.Count > 0)
            {
                Notes.UploadNotes();
            }
        }

        private void SetDueAndIsLate(DateTime? dueDateTime, bool hasDueTime)
        {
            IsLate = false;

            if (dueDateTime == null)
            {
                mDue = null;
                mDueDateTime = null;
                HasDueTime = false;
            }
            else
            {
                UserSettings userSettings = null;
                if (Owner != null)
                    userSettings = Owner.UserSettings;

                TimeSpan fromToday = dueDateTime.Value.Subtract(DateTime.Now);
                if (fromToday.Ticks < 0)
                {
                    IsLate = true;
                }

                HasDueTime = hasDueTime;
                mDueDateTime = dueDateTime;

                FuzzyDateTime fuzzyDueDateTime = new FuzzyDateTime(dueDateTime.Value, hasDueTime);
                mDue = DateConverter.FormatDateTime(fuzzyDueDateTime, 
                    userSettings != null ? userSettings.DateFormat : DateFormat.Default, 
                    userSettings != null ? userSettings.TimeFormat : TimeFormat.Default);
            }
        }

        private void SetRecurrence(RawRepeatRule rawRepeatRule)
        {
            if (rawRepeatRule == null)
            {
                mRecurrence = null;
                return;
            }


            DateFormat dateFormat = /*IsSynced ? Owner.UserSettings.DateFormat :*/ DateFormat.Default;
            mRecurrence = RecurrenceConverter.FormatRecurrence(rawRepeatRule.Rule, rawRepeatRule.Every == 1, dateFormat);
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

        public int CompareTo(object obj)
        {
            if (obj is Task)
            {
                Task other = obj as Task;

                return Task.CompareByDate(this, other);
            }
            else
            {
                throw new ArgumentException("Cannot compare Task to other types of objetcs.");
            }
        }

        public static int CompareByDate(Task a, Task b)
        {
            int cmp = 0;

            if (a.DueDateTime.HasValue && b.DueDateTime.HasValue)
            {
                cmp = a.FuzzyDueDateTime.CompareTo(b.FuzzyDueDateTime);
            }
            else
            {
                cmp = (b.DueDateTime.HasValue ? 1 : 0) - (a.DueDateTime.HasValue ? 1 : 0);
            }

            if (cmp == 0)
            {
                if (a.Priority != b.Priority)
                {
                    if (a.Priority == TaskPriority.None) cmp = 1;
                    else if (b.Priority == TaskPriority.None) cmp = -1;
                    else cmp = a.Priority.CompareTo(b.Priority);
                }
                else
                {
                    cmp = a.Name.CompareTo(b.Name);
                }
            }

            return cmp;
        }

        public static int CompareByPriority(Task a, Task b)
        {
            int cmp = 0;

            if (a.Priority != b.Priority)
            {
                if (a.Priority == TaskPriority.None) cmp = 1;
                else if (b.Priority == TaskPriority.None) cmp = -1;
                else cmp = a.Priority.CompareTo(b.Priority);
            }
            else
            {
                if (a.DueDateTime.HasValue && b.DueDateTime.HasValue)
                {
                    cmp = a.FuzzyDueDateTime.CompareTo(b.FuzzyDueDateTime);
                }
                else
                {
                    cmp = (b.DueDateTime.HasValue ? 1 : 0) - (a.DueDateTime.HasValue ? 1 : 0);
                }

                if (cmp == 0)
                {
                    cmp = a.Name.CompareTo(b.Name);
                }
            }

            return cmp;
        }

        public static int CompareByName(Task a, Task b)
        {
            int cmp = a.Name.CompareTo(b.Name);
            if (cmp == 0)
            {
                if (a.Priority != b.Priority)
                {
                    if (a.Priority == TaskPriority.None) cmp = 1;
                    else if (b.Priority == TaskPriority.None) cmp = -1;
                    else cmp = a.Priority.CompareTo(b.Priority);
                }
                else
                {
                    if (a.DueDateTime.HasValue && b.DueDateTime.HasValue)
                    {
                        cmp = a.FuzzyDueDateTime.CompareTo(b.FuzzyDueDateTime);
                    }
                    else
                    {
                        cmp = (b.DueDateTime.HasValue ? 1 : 0) - (a.DueDateTime.HasValue ? 1 : 0);
                    }
                }
            }

            return cmp;
        }
    }
}
