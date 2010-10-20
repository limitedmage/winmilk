using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IronCow.Rest
{
    public class RawNote : RawRtmElement
    {
        [XmlAttribute("created")]
        public string Created { get; set; }

        [XmlAttribute("modified")]
        public string Modified { get; set; }

        [XmlAttribute("title")]
        public string Title { get; set; }

        [XmlText]
        public string Body { get; set; }
    }
}
