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
using IronCow;
using Microsoft.Phone.Shell;

namespace WinMilk.Gui
{
    public partial class EditNotePage : PhoneApplicationPage
    {
        #region IsLoading Property

        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register("IsLoading", typeof(bool), typeof(EditNotePage),
                new PropertyMetadata((bool)false));

        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        #endregion

        public enum AddEditAction
        {
            Add,
            Edit
        };

        public AddEditAction Action { get; set; }
        public Task CurrentTask { get; set; }
        public TaskNote CurrentNote { get; set; }

        public EditNotePage()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            CreateApplicationBar();

            if (NavigationContext.QueryString.ContainsKey("task"))
            {
                CurrentTask = App.RtmClient.GetTask(NavigationContext.QueryString["task"]);
                if (CurrentTask == null)
                {
                    MessageBox.Show("..Error: No task selected");
                    NavigationService.GoBack();
                    return;
                }

                TaskName.Text = CurrentTask.NameUpper;
            }
            else
            {
                MessageBox.Show("..Error: No task selected");
                NavigationService.GoBack();
                return;
            }

            if (NavigationContext.QueryString["action"] == "edit")
            {
                Action = AddEditAction.Edit;
                AddEditTitle.Text = "..edit note";

                // TODO: load data from note
                if (NavigationContext.QueryString.ContainsKey("note"))
                {
                    CurrentNote = CurrentTask.GetNote(NavigationContext.QueryString["note"]);
                    if (CurrentNote == null)
                    {
                        MessageBox.Show("..Error: No note selected");
                        NavigationService.GoBack();
                        return;
                    }

                    NoteTitle.Text = CurrentNote.Title;
                    NoteBody.Text = CurrentNote.Body;
                }
                else
                {
                    MessageBox.Show("..Error: No note selected");
                    NavigationService.GoBack();
                    return;
                }
            }
            else
            {
                Action = AddEditAction.Add;
                AddEditTitle.Text = "..add note";
            }
        }

        private void CreateApplicationBar()
        {
            ApplicationBar = new ApplicationBar();

            ApplicationBarIconButton save = new ApplicationBarIconButton(new Uri("/icons/appbar.save.rest.png", UriKind.Relative));
            save.Text = AppResources.EditTaskSaveButton;
            save.Click += new EventHandler(Save_Click);
            ApplicationBar.Buttons.Add(save);
        }

        private void Note_Typed(object sender, KeyEventArgs e)
        {
            // tombstone data here!!
        }

        private void Save_Click(object sender, EventArgs e)
        {
            if (Action == AddEditAction.Edit)
            {
                IsLoading = true;
                (ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = false;
                NoteTitle.IsEnabled = false;
                NoteBody.IsEnabled = false;

                CurrentTask.EditNote(CurrentNote, NoteTitle.Text, NoteBody.Text, () =>
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        IsLoading = false;
                        NavigationService.GoBack();
                    });
                });
            }
        }
    }
}