using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace IronCow.Rest
{
    [DataContract]
    public class RawRtmElement
    {
        [XmlAttribute("id")]
        [DataMember]
        public int Id { get; set; }
    }
}
