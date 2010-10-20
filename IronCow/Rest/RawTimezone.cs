using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IronCow.Rest
{
    public class RawTimezone
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("dst")]
        public int Dst { get; set; }

        [XmlAttribute("offset")]
        public int Offset { get; set; }

        [XmlAttribute("current_offset")]
        public int CurrentOffset { get; set; }
    }
}
