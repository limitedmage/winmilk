using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IronCow.Rest
{
    public class RawAuthentication
    {
        [XmlElement("token")]
        public string Token { get; set; }

        [XmlElement("perms")]
        public AuthenticationPermissions Permissions { get; set; }

        [XmlElement("user")]
        public RawUser User { get; set; }
    }
}
