using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace WinMilk.Gui
{
    public partial class EditNotePage : PhoneApplicationPage
    {
        private enum AddEditAction
        {
            Add,
            Edit
        };

        private AddEditAction action;

        public EditNotePage()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (NavigationContext.QueryString["action"] == "add")
            {
                action = AddEditAction.Add;
                AddEditTitle.Text = "..add note";
            }
            else
            {
                action = AddEditAction.Edit;
                AddEditTitle.Text = "..edit note";

                // load data from note
            }
        }

        private void Note_Typed(object sender, KeyEventArgs e)
        {
            // tombstone data here!!
        }
    }
}