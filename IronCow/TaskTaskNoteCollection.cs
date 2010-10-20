using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace IronCow
{
    public class TaskTaskNoteCollection : SynchronizedTaskCollection<TaskNote>
    {
        #region Static Members
        private static RestRequest CreateAddTaskNoteRequest(Task task, TaskNote taskNote)
        {
            RestRequest request = new RestRequest("rtm.tasks.notes.add", r => taskNote.Sync(r.Note));
            request.Parameters.Add("timeline", task.Owner.GetTimeline().ToString());
            request.Parameters.Add("list_id", task.Parent.Id.ToString());
            request.Parameters.Add("taskseries_id", task.SeriesId.ToString());
            request.Parameters.Add("task_id", task.Id.ToString());
            request.Parameters.Add("note_title", taskNote.Title);
            request.Parameters.Add("note_text", taskNote.Body);
            return request;
        }

        private static RestRequest CreateDeleteTaskNoteRequest(Task task, TaskNote taskNote)
        {
            RestRequest request = new RestRequest("rtm.tasks.notes.delete", r => taskNote.Unsync());
            request.Parameters.Add("timeline", task.Owner.GetTimeline().ToString());
            request.Parameters.Add("note_id", taskNote.Id.ToString());
            return request;
        }

        private static MultiRequest CreateSetTaskNotesRequest(Task task)
        {
            MultiRequest request = new MultiRequest();
            foreach (var taskNote in task.Notes)
            {
                request.Requests.Add(CreateAddTaskNoteRequest(task, taskNote));
            }
            return request;
        }
        #endregion

        #region Public Properties
        public TaskNote this[int index]
        {
            get
            {
                lock (SyncRoot)
                {
                    return Items[index];
                }
            }
        }
        #endregion

        #region Construction
        internal TaskTaskNoteCollection(Task task)
            : base(task)
        {
        } 
        #endregion

        #region Public Methods
        //private void Resync(bool firstSync, RawTaskSeries series)
        //{
        //    if (series.Notes != null)
        //    {
        //        if (Notes.Count == 0)
        //        {
        //            foreach (var note in series.Notes)
        //            {
        //                Notes.UnsyncedAdd(new TaskNote(note));
        //            }
        //        }
        //        else
        //        {
        //            if (firstSync)
        //            {
        //                // If this is the first sync, clear the previously defined notes.
        //                // They've been saved in the FirstSyncTaskData, and will be re-added
        //                // later.
        //                Notes.UnsyncedClear();
        //            }

        //            int[] remoteNoteIds = new int[series.Notes.Length];
        //            for (int i = 0; i < series.Notes.Length; i++)
        //            {
        //                remoteNoteIds[i] = series.Notes[i].Id;
        //            }

        //            // Look for each task we have and see if it still exists.
        //            // If not, remove it.
        //            List<TaskNote> taskNotesToRemove = new List<TaskNote>();
        //            foreach (var taskNote in Notes)
        //            {
        //                if (Array.IndexOf(remoteNoteIds, taskNote.Id) < 0)
        //                {
        //                    taskNotesToRemove.Add(taskNote);
        //                }
        //            }
        //            foreach (var taskNote in taskNotesToRemove)
        //            {
        //                Notes.UnsyncedRemove(taskNote);
        //                taskNote.Unsync();
        //            }
        //            // Look for each task that the server add and see if we have it.
        //            // If not, add it.
        //            foreach (var note in series.Notes)
        //            {
        //                Func<TaskNote, bool> criteria = (tn => tn.Id == note.Id);
        //                TaskNote taskNote = Notes.First(criteria);
        //                if (taskNote == null)
        //                {
        //                    taskNote = new TaskNote(note);
        //                    Notes.UnsyncedAdd(taskNote);
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        Notes.UnsyncedClear();
        //    }
        //}
        #endregion

        #region Internal Methods
        internal void UploadNotes()
        {
            if (Count == 0)
                return;

            Request request = CreateSetTaskNotesRequest(Task);
            Task.Owner.ExecuteRequest(request);
        }

        internal Request GetFirstSyncRequest()
        {
            if (Count == 0)
                return null;

            return CreateSetTaskNotesRequest(Task);
            
        }

        internal void OnOwnerChanged()
        {
            foreach (var item in this)
            {
                item.Owner = Task.Owner;
            }
        }
        #endregion

        #region SynchronizedTaskCollection<TaskNote> Members
        protected override void OnSyncingChanged()
        {
            foreach (var note in this)
            {
                note.Syncing = Syncing;
            }
            base.OnSyncingChanged();
        }

        protected override Request CreateClearItemsRequest()
        {
            MultiRequest request = new MultiRequest();
            foreach (var taskNote in this)
            {
                request.Requests.Add(CreateDeleteTaskNoteRequest(Task, taskNote));
            }
            return request;
        }

        protected override Request CreateAddItemRequest(TaskNote item)
        {
            return CreateAddTaskNoteRequest(Task, item);
        }

        protected override Request CreateRemoveItemRequest(TaskNote item)
        {
            return CreateDeleteTaskNoteRequest(Task, item);
        }

        protected override void ClearItems()
        {
            foreach (var item in this)
            {
                item.Owner = null;
            }
            base.ClearItems();
        }

        protected override void AddItem(TaskNote item)
        {
            item.Owner = Task.Owner;
            base.AddItem(item);
        }

        protected override void RemoveItem(TaskNote item)
        {
            item.Owner = null;
            base.RemoveItem(item);
        }
        #endregion
    }
}
