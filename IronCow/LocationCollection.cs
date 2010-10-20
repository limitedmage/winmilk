using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using IronCow.Rest;

namespace IronCow
{
    public class LocationCollection : RtmCollection<Location>
    {
        internal LocationCollection(Rtm owner)
            : base(owner)
        {
        }

        internal void Resync()
        {
            if (Owner.Syncing)
            {
                Items.Clear();
                var request = new RestRequest("rtm.locations.getList");
                request.Callback = response =>
                    {
                        if (response.Locations != null)
                        {
                            foreach (var location in response.Locations)
                            {
                                Items.Add(new Location(location));
                            }
                        }
                    };
                Owner.ExecuteRequest(request);
            }
        }
    }
}
