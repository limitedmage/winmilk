using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IronCow.Rest
{
    public class RawTask : RawRtmElement
    {
        [XmlAttribute("due")]
        public string Due { get; set; }

        [XmlAttribute("has_due_time")]
        public int HasDueTime { get; set; }

        [XmlAttribute("added")]
        public string Added { get; set; }

        [XmlAttribute("completed")]
        public string Completed { get; set; }

        [XmlAttribute("deleted")]
        public string Deleted { get; set; }

        // N, 1, 2, 3
        [XmlAttribute("priority")]
        public string Priority { get; set; }

        [XmlAttribute("postponed")]
        public string Postponed { get; set; }

        [XmlAttribute("estimate")]
        public string Estimate { get; set; }
    }
}
