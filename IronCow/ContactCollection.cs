using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using IronCow.Rest;

namespace IronCow
{
    public class ContactCollection : SynchronizedRtmCollection<Contact>
    {
        internal ContactCollection(Rtm owner)
            : base(owner)
        {
        }

        protected override void DoResync()
        {
            Clear();
            var request = new RestRequest("rtm.contacts.getList");
            request.Callback = response =>
                {
                    if (response.Contacts != null)
                    {
                        using (new UnsyncedScope(this))
                        {
                            foreach (var contact in response.Contacts)
                            {
                                Add(new Contact(contact));
                            }
                        }
                    }
                };
            Owner.ExecuteRequest(request);
        }

        protected override void ExecuteAddElementRequest(Contact item)
        {
            if (item == null)
                throw new ArgumentNullException("contact");
            RestRequest request = new RestRequest("rtm.contacts.add", r => item.Sync(r.Contact));
            request.Parameters.Add("contact", item.UserName);
            request.Parameters.Add("timeline", Owner.GetTimeline().ToString());
            Owner.ExecuteRequest(request);
        }

        protected override void ExecuteRemoveElementRequest(Contact item)
        {
            if (item == null)
                throw new ArgumentNullException("contact");
            RestRequest request = new RestRequest("rtm.contacts.delete", r => item.Unsync());
            request.Parameters.Add("contact_id", item.Id.ToString());
            request.Parameters.Add("timeline", Owner.GetTimeline().ToString());
            Owner.ExecuteRequest(request);
        }
    }
}
