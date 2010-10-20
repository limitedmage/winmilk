using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IronCow.Rest
{
    public class RawRepeatRule
    {
        [XmlAttribute("every")]
        public int Every { get; set; }

        [XmlText]
        public string Rule { get; set; }
    }
}
