using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IronCow.Rest
{
    public class RawTransaction
    {
        [XmlAttribute("id")]
        public string IdString { get; set; }

        public int? Id
        {
            get
            {
                if (string.IsNullOrEmpty(IdString))
                    return null;
                return int.Parse(IdString);
            }
        }

        [XmlAttribute("undoable")]
        public int Undoable { get; set; }
    }
}
