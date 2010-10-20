using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IronCow
{
    public enum AuthenticationPermissions
    {
        [XmlEnum("read")]
        Read,
        [XmlEnum("write")]
        Write,
        [XmlEnum("delete")]
        Delete
    }
}
