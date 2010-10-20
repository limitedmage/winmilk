using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IronCow.Rest
{
    public class RawLocation : RawRtmElement
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("longitude")]
        public float Longitude { get; set; }

        [XmlAttribute("latitude")]
        public float Latitude { get; set; }

        [XmlAttribute("zoom")]
        public int Zoom { get; set; }

        [XmlAttribute("address")]
        public string Address { get; set; }

        [XmlAttribute("viewable")]
        public int Viewable { get; set; }
    }
}
