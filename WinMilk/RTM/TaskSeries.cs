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
	public class TaskSeries {
		
		private int id;
		public int Id {
			get { return this.id; }
		}

		private DateTime created;
		public DateTime Created {
			get { return this.created; }
		}
		
		private DateTime modified;
		public DateTime Modififed {
			get { return this.modified; }
		}
		
		private string name;
		public string Name {
			get { return this.name; }
		}

		private LinkedList<string> tags;
		public LinkedList<string> Tags {
			get { return this.tags; }
		}

		private LinkedList<string> notes;
		public LinkedList<string> Notes {
			get { return this.notes; }
		}

		private LinkedList<Task> tasks;
		public LinkedList<Task> Tasks {
			get { return this.tasks; }
		}
	}
}
