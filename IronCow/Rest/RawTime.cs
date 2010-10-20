using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IronCow.Rest
{
    public class RawTime
    {
        [XmlAttribute("precision")]
        public string Precision { get; set; }

        [XmlText]
        public DateTime Time { get; set; }
    }
}
