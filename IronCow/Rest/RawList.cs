using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IronCow.Rest
{
    public class RawList : RawRtmElement
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("filter")]
        public string Filter { get; set; }

        [XmlAttribute("deleted")]
        public int Deleted { get; set; }

        [XmlAttribute("locked")]
        public int Locked { get; set; }

        [XmlAttribute("archived")]
        public int Archived { get; set; }

        [XmlAttribute("position")]
        public int Position { get; set; }

        [XmlAttribute("smart")]
        public int Smart { get; set; }

        [XmlElement("taskseries")]
        public RawTaskSeries[] TaskSeries { get; set; }

        [XmlElement("sort_order")]
        public int SortOrder { get; set; }

        [XmlArray("deleted")]
        [XmlArrayItem("taskseries")]
        public RawTaskSeries[] DeletedTaskSeries { get; set; }
    }
}
