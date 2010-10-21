using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace IronCow.Rest
{
    [DataContract]
    public class RawList : RawRtmElement
    {
        [DataMember]
        [XmlAttribute("name")]
        public string Name { get; set; }

        [DataMember]
        [XmlElement("filter")]
        public string Filter { get; set; }

        [DataMember]
        [XmlAttribute("deleted")]
        public int Deleted { get; set; }

        [DataMember]
        [XmlAttribute("locked")]
        public int Locked { get; set; }

        [DataMember]
        [XmlAttribute("archived")]
        public int Archived { get; set; }

        [DataMember]
        [XmlAttribute("position")]
        public int Position { get; set; }

        [DataMember]
        [XmlAttribute("smart")]
        public int Smart { get; set; }

        [DataMember]
        [XmlElement("taskseries")]
        public RawTaskSeries[] TaskSeries { get; set; }

        [DataMember]
        [XmlArray("deleted")]
        [XmlArrayItem("taskseries")]
        public RawTaskSeries[] DeletedTaskSeries { get; set; }
    }
}
