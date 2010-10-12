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
    public class TaskList
    {
        [DataMember]
        public LinkedList<Task> List { get; set; }
        
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        public TaskList(int id, string name)
        {
            this.Id = id;
            this.Name = name;
        }

        public void add(Task t)
        {

        }

        public void complete(Task t)
        {

        }
    }
}
