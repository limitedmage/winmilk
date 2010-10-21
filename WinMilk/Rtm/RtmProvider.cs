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
using System.Collections.ObjectModel;
using IronCow.Rest;
using IronCow.Search;

namespace WinMilk.Rtm
{
    public static class RtmProvider
    {
        private static readonly string RtmApiKey = "624a977e8a0e4ce69dec0196ce6479dd";
        private static readonly string RtmSharedKey = "ea04c412d8a8ce87";

        private static string RtmToken;

        public static RestClient RtmClient;

        public static ObservableCollection<TaskViewModel> Tasks;
        public static ObservableCollection<ListViewModel> Lists;

        public static void DownloadAll()
        {
            if (RtmClient != null && !string.IsNullOrEmpty(RtmClient.AuthToken))
            {
                RtmClient.GetLists(lists =>
                {
                    foreach (RawList list in lists)
                    {
                        ListViewModel l = new ListViewModel(list);
                        Lists.Add(l);
                    }

                    RtmClient.GetTasks("status:incomplete", tempLists =>
                    {
                        foreach (RawList list in tempLists)
                        {
                            ListViewModel parent = GetList(list.Id);

                            foreach (RawTaskSeries series in list.TaskSeries)
                            {
                                foreach (RawTask task in series.Tasks)
                                {
                                    TaskViewModel t = new TaskViewModel(parent, series, task);
                                    Tasks.Add(t);
                                }
                            }
                        }
                    });
                });
            }
        }

        public static ListViewModel GetList(int id)
        {
            foreach (ListViewModel l in Lists)
            {
                if (l.Id == id)
                {
                    return l;
                }
            }

            return null;
        }

        public static ObservableCollection<TaskViewModel> GetTasksFromFilter(string filter)
        {
            ObservableCollection<TaskViewModel> result = new ObservableCollection<TaskViewModel>();

            try
            {
                var lexicalAnalyzer = new LexicalAnalyzer();
                var tokens = lexicalAnalyzer.Tokenize(filter);
                var astRoot = lexicalAnalyzer.BuildAst(tokens);

                bool includeArchivedLists = astRoot.NeedsArchivedLists();

                foreach (var list in searchableTaskLists)
                {
                    foreach (var task in list.Tasks)
                    {
                        var context = new Search.SearchContext(task, UserSettings.DateFormat);
                        if (astRoot.ShouldInclude(context))
                            resultTasks.Add(task);
                    }
                }
            }
            catch (Exception e)
            {

            }

            return result;
        }

        public static void LoadData()
        {
            string storedToken = Helper.IsolatedStorageHelper.GetObject<string>("RtmToken");

            if (!string.IsNullOrEmpty(storedToken))
            {
                RtmToken = storedToken;
                RtmClient = new RestClient(RtmApiKey, RtmSharedKey, storedToken);
            }
            else
            {
                RtmToken = null;
                RtmClient = new RestClient(RtmApiKey, RtmSharedKey);
            }

            ObservableCollection<TaskViewModel> storedTasks = Helper.IsolatedStorageHelper.GetObject<ObservableCollection<TaskViewModel>>("Tasks");

            if (!(storedTasks == null || storedTasks.Count == 0))
            {
                Tasks = storedTasks;
            }
            else
            {
                Tasks = null;
            }

            ObservableCollection<ListViewModel> storedLists = Helper.IsolatedStorageHelper.GetObject<ObservableCollection<ListViewModel>>("Lists");

            if (!(storedLists == null || storedLists.Count == 0))
            {
                Lists = storedLists;
            }
            else
            {
                Lists = null;
            }
        }

        public static void SaveData()
        {
            Helper.IsolatedStorageHelper.SaveObject<string>("RtmToken", RtmToken);
            Helper.IsolatedStorageHelper.SaveObject<ObservableCollection<TaskViewModel>>("Tasks", Tasks);
            Helper.IsolatedStorageHelper.SaveObject<ObservableCollection<ListViewModel>>("Lists", Lists);
        }

        public static void DeleteData()
        {
            Helper.IsolatedStorageHelper.DeleteObject("RtmToken");
            Helper.IsolatedStorageHelper.DeleteObject("Tasks");
            Helper.IsolatedStorageHelper.DeleteObject("Lists");
        }
    }
}
