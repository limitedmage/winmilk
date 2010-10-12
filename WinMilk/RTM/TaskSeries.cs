using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WinMilk.RTM
{
    [DataContract]
    public class TaskSeries
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public DateTime Created { get; set; }

        [DataMember]
        public DateTime Modififed { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public LinkedList<string> Tags { get; set; }

        [DataMember]
        public LinkedList<string> Notes { get; set; }

        [DataMember]
        public LinkedList<Task> Tasks { get; set; }
    }
}
