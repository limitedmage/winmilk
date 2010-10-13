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
    public class Task
    {

        [DataMember]
        public int ListId { get; set; }

        [DataMember]
        public string List { get; set; }

        [DataMember]
        public int TaskSeriesId { get; set; }

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public int Priority { get; set; }

        public string PriorityColor
        {
            get
            {
                switch (Priority)
                {
                    case 1:
                        return "#EA5200";
                    case 2:
                        return "#0060BF";
                    case 3:
                        return "#359AFF";
                    default:
                        return Colors.Transparent.ToString();
                }
            }
        }

        [DataMember]
        public int Postponed { get; set; }

        [DataMember]
        public DateTime Added { get; set; }

        [DataMember]
        public bool HasDueTime { get; set; }

        [DataMember]
        public bool HasDue { get; set; }

        [DataMember]
        public DateTime Due { get; set; }

        public string DueDateString
        {
            get
            {
                string dueString = "";
                if (this.HasDue)
                {
                    if (this.Due.Date == DateTime.Today)
                    {
                        dueString += "Today";
                    }
                    else if (DateTime.Today.AddDays(1) == this.Due.Date)
                    {
                        dueString += "Tomorrow";
                    }
                    else if (DateTime.Today.AddDays(6) >= this.Due.Date)
                    {
                        dueString += this.Due.ToString("dddd");
                    }
                    else
                    {
                        dueString += this.Due.ToString("ddd d MMM");
                    }

                    if (this.HasDueTime)
                    {
                        dueString += " " + this.Due.ToString("t");
                    }
                }

                return dueString;
            }
        }

        public string LongDueDateString
        {
            get
            {
                string dueString = "Never";
                if (this.HasDue)
                {
                    if (this.HasDueTime)
                    {
                        dueString = this.Due.ToString("f");
                    }
                    else
                    {
                        dueString = this.Due.ToString("D");
                    }
                }

                return dueString;
            }
        }

        [DataMember]
        public DateTime Deleted { get; set; }

        [DataMember]
        public string Estimate { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public List<string> Tags { get; set; }

        public string TagsString
        {
            get
            {
                return string.Join(", ", Tags.ToArray());
            }
        }

        public bool HasTags
        {
            get
            {
                return Tags.Count != 0;
            }
        }

        public Task()
            : this(0, 0, 0, "", new List<string>(), 0, "", false, "")
        {
        }

        public Task(int id, int listId, int taskSeriedId, string name, List<string> tags, int priority, string list, bool hasDueTime, string due)
        {
            Id = id;
            ListId = listId;
            TaskSeriesId = taskSeriedId;
            Name = name;
            Tags = tags;
            Priority = priority;
            List = list;
            HasDueTime = hasDueTime;

            if (due == "")
            {
                // empty due date means no due date
                HasDue = false;
            }
            else
            {
                Due = DateTime.Parse(due);
                HasDue = true;
            }
        }

        public static int StringToPriority(string priority)
        {
            if (priority == "1") return 1;
            else if (priority == "2") return 2;
            else if (priority == "3") return 3;
            else return 0;
        }
    }
}
