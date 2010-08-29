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

namespace PhoneMilk.RTM {
	public class TaskList {
		private LinkedList<Task> list;
		public LinkedList<Task> List 
		{
			get { return this.list; }
		}

		private int id;
		public int Id
		{
			get { return this.id; }
		}

		private string name;
		public string Name
		{
			get { return this.name; }
		}

		public TaskList(int id, string name) 
		{
			this.id = id;
			this.name = name;
		}

		public void add(Task t) 
		{ 
			
		}

		public void complete(Task t) 
		{ 
			
		}
	}
}
