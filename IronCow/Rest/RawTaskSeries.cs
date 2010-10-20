using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IronCow.Rest
{
    public class RawTaskSeries : RawRtmElement
    {
        [XmlAttribute("created")]
        public string Created { get; set; }

        [XmlAttribute("modified")]
        public string Modified { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("source")]
        public string Source { get; set; }

        [XmlAttribute("url")]
        public string Url { get; set; }

        [XmlAttribute("location_id")]
        public string LocationId { get; set; }

        [XmlElement("rrule")]
        public RawRepeatRule RepeatRule { get; set; }

        [XmlArray("tags")]
        [XmlArrayItem("tag")]
        public string[] Tags { get; set; }

        //TODO: participants

        [XmlArray("notes")]
        [XmlArrayItem("note")]
        public RawNote[] Notes { get; set; }

        [XmlElement("task")]
        public RawTask[] Tasks { get; set; }
    }
}
