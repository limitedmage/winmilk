using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace IronCow.Rest
{
    [DataContract]
    public class RawNote : RawRtmElement
    {
        [DataMember]
        [XmlAttribute("created")]
        public string Created { get; set; }

        [DataMember]
        [XmlAttribute("modified")]
        public string Modified { get; set; }

        [DataMember]
        [XmlAttribute("title")]
        public string Title { get; set; }

        [DataMember]
        [XmlText]
        public string Body { get; set; }
    }
}
