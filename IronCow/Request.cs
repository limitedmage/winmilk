using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using IronCow.Rest;

namespace IronCow
{
    public abstract class Request
    {
        protected Request()
        {
        }

        public abstract void Execute(Rtm rtm);
    }
}
