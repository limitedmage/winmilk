using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IronCow.Rest
{
    public class RawSettings
    {
        [XmlElement("timezone")]
        public string TimeZone { get; set; }

        [XmlElement("dateformat")]
        public int DateFormat { get; set; }

        [XmlElement("timeformat")]
        public int TimeFormat { get; set; }

        [XmlElement("defaultlist")]
        public string DefaultList { get; set; }

        [XmlElement("language")]
        public string Language { get; set; }
    }
}
