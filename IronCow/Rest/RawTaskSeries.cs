using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace IronCow.Rest
{
    [DataContract]
    public class RawTaskSeries : RawRtmElement
    {
        [DataMember]
        [XmlAttribute("created")]
        public string Created { get; set; }
        
        [DataMember]
        [XmlAttribute("modified")]
        public string Modified { get; set; }

        [DataMember]
        [XmlAttribute("name")]
        public string Name { get; set; }

        [DataMember]
        [XmlAttribute("source")]
        public string Source { get; set; }

        [DataMember]
        [XmlAttribute("url")]
        public string Url { get; set; }

        [DataMember]
        [XmlAttribute("location_id")]
        public string LocationId { get; set; }

        [DataMember]
        [XmlElement("rrule")]
        public RawRepeatRule RepeatRule { get; set; }

        [DataMember]
        [XmlArray("tags")]
        [XmlArrayItem("tag")]
        public string[] Tags { get; set; }

        //TODO: participants

        [DataMember]
        [XmlArray("notes")]
        [XmlArrayItem("note")]
        public RawNote[] Notes { get; set; }

        [DataMember]
        [XmlElement("task")]
        public RawTask[] Tasks { get; set; }
    }
}
