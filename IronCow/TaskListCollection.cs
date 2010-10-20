using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using IronCow.Rest;

namespace IronCow
{
    public class TaskListCollection : SynchronizedRtmCollection<TaskList>
    {
        public TaskList this[string listName]
        {
            get
            {
                foreach (var item in this)
                {
                    if (item.Name == listName)
                        return item;
                }
                throw new IronCowException(string.Format("No task list with such name: '{0}'.", listName));
            }
        }

        internal TaskListCollection(Rtm owner)
            : base(owner)
        {
        }

        protected override void DoResync()
        {
            Clear();
            var request = new RestRequest("rtm.lists.getList");
            request.Callback = response =>
                {
                    if (response.Lists != null)
                    {
                        using (new UnsyncedScope(this))
                        {
                            foreach (var list in response.Lists)
                            {
                                Add(new TaskList(list));
                            }
                        }
                    }
                };
            Owner.ExecuteRequest(request);
        }

        protected override void ExecuteAddElementRequest(TaskList item)
        {
            if (string.IsNullOrEmpty(item.Name))
                throw new ArgumentException("The task list has a null or empty name.");

            RestRequest request = new RestRequest("rtm.lists.add", r => item.Sync(r.List));
            request.Parameters.Add("timeline", Owner.GetTimeline().ToString());
            request.Parameters.Add("name", item.Name);
            if (!string.IsNullOrEmpty(item.Filter))
            {
                request.Parameters.Add("filter", item.Filter);
            }
            Owner.ExecuteRequest(request);
        }

        protected override void ExecuteRemoveElementRequest(TaskList item)
        {
            if (item.IsSynced)
            {
                RestRequest request = new RestRequest("rtm.lists.delete", r => item.Sync(r.List));
                request.Parameters.Add("timeline", Owner.GetTimeline().ToString());
                request.Parameters.Add("list_id", item.Id.ToString());
                Owner.ExecuteRequest(request);
            }
        }
    }
}
