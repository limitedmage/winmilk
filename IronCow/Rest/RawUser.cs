using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IronCow.Rest
{
    public class RawUser
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlAttribute("username")]
        public string UserName { get; set; }

        [XmlAttribute("fullname")]
        public string FullName { get; set; }
    }
}
