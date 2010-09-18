using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace WinMilk.Gui.Loc
{
	public class LocData : INotifyPropertyChanged
	{
		public ObservableCollection<LocString> Strings { get; private set; }

		public LocData()
		{
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged(String propertyName)
		{
			if (null != PropertyChanged)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public string GetString(string key) 
		{
			foreach (LocString locString in Strings)
			{
				if (key == locString.Key)
				{
					return locString.Value;
				}
			}

			return "..unknown string";
		}

		public string this[string s]
		{
			get
			{
				return GetString(s);
			}
		}
	}
}
