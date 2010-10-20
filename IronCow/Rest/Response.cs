using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace IronCow.Rest
{
    [XmlRoot("rsp", Namespace = "", IsNullable = false)]
    [DataContract]
    public class Response
    {
        [XmlAttribute("stat", Form = XmlSchemaForm.Unqualified)]
        [DefaultValue(ResponseStatus.Unknown)]
        public ResponseStatus Status = ResponseStatus.Unknown;

        [XmlElement("method", Form = XmlSchemaForm.Unqualified)]
        public string Method;

        [XmlElement("err", Form = XmlSchemaForm.Unqualified)]
        public ResponseError Error;

        [XmlElement("frob")]
        public string Frob;

        [XmlElement("auth")]
        public RawAuthentication Authentication;

        [XmlElement("transaction")]
        public RawTransaction Transaction;

        [XmlArray("contacts")]
        [XmlArrayItem("contact")]
        public RawContact[] Contacts;

        [XmlElement("contact")]
        public RawContact Contact;

        [XmlArray("groups")]
        [XmlArrayItem("group")]
        public RawGroup[] Groups;

        [XmlElement("group")]
        public RawGroup Group;

        [XmlArray("locations")]
        [XmlArrayItem("location")]
        public RawLocation[] Locations;

        [XmlArray("lists")]
        [XmlArrayItem("list")]
        public RawList[] Lists;

        [XmlElement("list")]
        public RawList List;

        [XmlArray("tasks")]
        [XmlArrayItem("list")]
        public RawList[] Tasks;

        [XmlElement("note")]
        public RawNote Note;

        [XmlElement("settings")]
        public RawSettings Settings;

        [XmlElement("user")]
        public RawUser User;

        [XmlElement("time")]
        public RawTime Time;

        [XmlElement("timeline")]
        public int Timeline;

        [XmlArray("timezones")]
        [XmlArrayItem("timezone")]
        public RawTimezone[] Timezones;
    }
}
