using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace IronCow.Rest
{
    [DataContract]
    public class RawTask : RawRtmElement
    {
        [DataMember]
        [XmlAttribute("due")]
        public string Due { get; set; }

        [DataMember]
        [XmlAttribute("has_due_time")]
        public int HasDueTime { get; set; }

        [DataMember]
        [XmlAttribute("added")]
        public string Added { get; set; }

        [DataMember]
        [XmlAttribute("completed")]
        public string Completed { get; set; }

        [DataMember]
        [XmlAttribute("deleted")]
        public string Deleted { get; set; }

        // N, 1, 2, 3
        [DataMember]
        [XmlAttribute("priority")]
        public string Priority { get; set; }

        [DataMember]
        [XmlAttribute("postponed")]
        public string Postponed { get; set; }

        [DataMember]
        [XmlAttribute("estimate")]
        public string Estimate { get; set; }
    }
}
