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
    public enum TaskListSortOrder { Priority, Date, Name }

    [DataContract]
    public class TaskList : IComparable
    {
        [DataMember]
        public LinkedList<Task> List { get; set; }

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public bool IsSmart { get; set; }
        public bool IsNormal { get { return !IsSmart; } }

        [DataMember]
        public string Filter { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public TaskListSortOrder SortOrder { get; set; }

        public TaskList(int id, string name, bool isSmart, string filter, TaskListSortOrder sortOrder)
        {
            Id = id;
            Name = name;
            IsSmart = isSmart;
            Filter = filter;
            SortOrder = sortOrder;
        }

        public static TaskListSortOrder ParseSortOrder(string str)
        {
            if (str == "0") return TaskListSortOrder.Priority;
            if (str == "1") return TaskListSortOrder.Date;
            return TaskListSortOrder.Name;
        }

        public int CompareTo(object obj)
        {
            TaskList other = obj as TaskList;

            if (this.Name == "Inbox") return -1;
            if (other.Name == "Inbox") return 1;
            if (this.Name == "Sent") return 1;
            if (other.Name == "Sent") return -1;

            if (this.IsSmart != other.IsSmart)
            {
                if (this.IsSmart) return 1;
                else return -1;
            }
            else
            {
                return this.Name.CompareTo(other.Name);
            }
        }
    }
}
