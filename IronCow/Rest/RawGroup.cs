using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IronCow.Rest
{
    public class RawGroup : RawRtmElement
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlArray("contacts")]
        [XmlArrayItem("contact")]
        public RawContact[] Contacts { get; set; }
    }
}
