using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace IronCow
{
    public class TaskTagCollection : SynchronizedTaskCollection<string>, IList<string>
    {
        #region Static Methods
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

        private static RestRequest CreateSetTagsRequest(Task task, IEnumerable<string> tags, VoidCallback callback)
        {
            StringBuilder tagsBuilder = new StringBuilder();
            foreach (string tag in tags)
            {
                if (tagsBuilder.Length > 0)
                    tagsBuilder.Append(",");
                tagsBuilder.Append(tag);
            }

            return CreateSetTagsRequest(task, tagsBuilder.ToString(), callback);
        }

        private static RestRequest CreateSetTagsRequest(Task task, string formattedTags, VoidCallback callback)
        {
            RestRequest request = CreateStandardRequest(task, "rtm.tasks.setTags", callback);
            request.Parameters.Add("tags", formattedTags);
            return request;
        }

        private static RestRequest CreateAddTagsRequest(Task task, string formattedTags, VoidCallback callback)
        {
            RestRequest request = CreateStandardRequest(task, "rtm.tasks.addTags", callback);
            request.Parameters.Add("tags", formattedTags);
            return request;
        }

        private static RestRequest CreateRemoveTagsRequest(Task task, string formattedTags, VoidCallback callback)
        {
            RestRequest request = CreateStandardRequest(task, "rtm.tasks.removeTags", callback);
            request.Parameters.Add("tags", formattedTags);
            return request;
        }
        #endregion

        #region Construction
        internal TaskTagCollection(Task task)
            : base(task)
        {
        } 
        #endregion

        #region Public Convenience Methods
        public void SetTags(IEnumerable<string> tags, VoidCallback callback)
        {
            using (new UnsyncedScope(this))
            {
                Clear(() =>
                {
                    if (tags != null)
                    {
                        foreach (string tag in tags)
                        {
                            Add(tag, () => { });
                        }
                    }

                    if (Task.IsSynced)
                        UploadTags(callback);
                });
            }
        }

        public void AddRange(IEnumerable<string> tags, VoidCallback callback)
        {
            using (new UnsyncedScope(this))
            {
                bool added = false;
                if (tags != null)
                {
                    foreach (string tag in tags)
                    {
                        added = true;
                        Add(tag, () => { });
                    }
                }

                if (added && Task.IsSynced)
                    UploadTags(callback);
            }
        } 
        #endregion

        #region Internal and Private Methods
        internal void UploadTags(VoidCallback callback)
        {
            RestRequest request = CreateSetTagsRequest(Task, this, callback);
            Task.Owner.ExecuteRequest(request);
        }

        internal RestRequest GetFirstSyncRequest(VoidCallback callback)
        {
            if (Count == 0)
                return null;

            return CreateSetTagsRequest(Task, this, callback);
        }
        #endregion

        #region SynchronizedTaskCollection<string> Members
        protected override Request CreateClearItemsRequest(VoidCallback callback)
        {
            return CreateSetTagsRequest(Task, string.Empty, callback);
        }

        protected override Request CreateAddItemRequest(string item, VoidCallback callback)
        {
            return CreateAddTagsRequest(Task, item, callback);
        }

        protected override Request CreateRemoveItemRequest(string item, VoidCallback callback)
        {
            return CreateRemoveTagsRequest(Task, item, callback);
        }
        #endregion

        #region IList<string> Members
        public int IndexOf(string item)
        {
            lock (SyncRoot)
            {
                return Items.IndexOf(item);
            }
        }

        public void Insert(int index, string item, VoidCallback callback)
        {
            lock (SyncRoot)
            {
                Items.Insert(index, item);
                UploadTags(callback);
            }
        }

        public void RemoveAt(int index, VoidCallback callback)
        {
            lock (SyncRoot)
            {
                Items.RemoveAt(index);
                UploadTags(callback);
            }
        }

        public string this[int index]
        {
            get
            {
                lock (SyncRoot)
                {
                    return Items[index];
                }
            }
            set
            {
                lock (SyncRoot)
                {
                    Items[index] = value;
                    UploadTags(() => {});
                }
            }
        }

        #endregion
    }
}
