using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronCow
{
    public abstract class SynchronizedRtmCollection<T> : RtmCollection<T>, ISyncing where T : RtmFatElement
    {
        private bool mSyncing;
        internal bool Syncing
        {
            get { return mSyncing; }
            set
            {
                mSyncing = value;
                OnSyncingChanged();
            }
        }

        protected SynchronizedRtmCollection(Rtm owner)
            : base(owner)
        {
            if (owner != null)
                Syncing = owner.Syncing;
            else
                Syncing = true;
        }

        public void Resync()
        {
            if (Syncing)
            {
                using (new UnsyncedScope(this))
                {
                    DoResync();
                }
            }
        }

        protected virtual void OnSyncingChanged()
        {
            foreach (var item in this)
            {
                item.Syncing = Syncing;
            }
        }

        protected override void ClearItems()
        {
            if (Syncing)
            {
                foreach (var item in this)
                {
                    if (item != null)
                    {
                        ExecuteRemoveElementRequest(item);
                    }
                }
            }
            base.ClearItems();
        }

        protected override void AddItem(T item)
        {
            if (item != null && Syncing)
                ExecuteAddElementRequest(item);
            base.AddItem(item);
        }

        protected override void RemoveItem(T item)
        {
            if (item != null && Syncing)
                ExecuteRemoveElementRequest(item);
            base.RemoveItem(item);
        }

        protected abstract void DoResync();

        protected abstract void ExecuteAddElementRequest(T item);

        protected abstract void ExecuteRemoveElementRequest(T item);

        #region ISyncing Members

        bool ISyncing.Syncing
        {
            get { return Syncing; }
            set { Syncing = value; }
        }

        #endregion
    }
}
