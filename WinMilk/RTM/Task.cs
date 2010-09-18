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

namespace WinMilk.RTM {
	public class Task {

		private int listId;
		public int ListId 
		{
			get { return this.listId; }
		}

		private string list;
		public string List
		{
			get { return this.list; }
		}

		private int taskSeriesId;
		public int TaskSeriesId 
		{
			get { return this.taskSeriesId; }
		}

		private int id;
		public int Id 
		{
			get { return this.id; }
		}

		private int priority;
		public int Priority {
			get { return this.priority; }
		}

		public string PriorityColor
		{
			get
			{
				switch (priority)
				{
					case 1:
						return "#EA5200";
					case 2:
						return "#0060BF";
					case 3:
						return "#359AFF";
					default:
						return SystemColors.WindowColor.ToString();
				}
			}
		}
		
		private int postponed;
		public int Postponed {
			get { return this.postponed; }
		}

		private DateTime added;
		public DateTime Added {
			get { return this.added; }
		}

		private bool hasDueTime;
		private bool hasDue;
		private DateTime due;

		public DateTime Due 
		{
			get
			{
				return this.due;
			}
		}

		public string DueDateString
		{
			get
			{
				if (this.hasDue) 
				{
					if (this.hasDueTime)
					{
						return this.due.ToString("d MMM h:mm tt");
					}
					else 
					{
						return this.due.ToString("d MMM");
					}
				}
				else
				{
					return "";
				}
			}
		}

		private DateTime deleted;
		public DateTime Deleted {
			get { return this.deleted; }
		}

		

		private string estimate;
		public string Estimate {
			get { return this.estimate; }
		}

		private string name;
		public string Name {
			get { return this.name; }
		}

		private List<string> tags;
		public List<string> Tags {
			get { return this.tags; }
		}
		public string TagsString
		{
			get
			{
				return string.Join(", ", tags.ToArray());
			}
		}

		public Task(string name, List<string> tags, int priority, string list, bool hasDueTime, string due) {
			this.name = name;
			this.tags = tags;
			this.priority = priority;
			this.list = list;
			this.hasDueTime = hasDueTime;

			if (due == "")
			{
				// empty due date means no due date
				this.hasDue = false;
			}
			else
			{
				this.due = DateTime.Parse(due);
				this.hasDue = true;
			}
		}

		public static int StringToPriority(string priority) {
			if (priority == "1") return 1;
			else if (priority == "2") return 2;
			else if (priority == "3") return 3;
			else return 0;
		}
	}
}
