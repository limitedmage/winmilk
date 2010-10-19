using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Linq;
using System.Xml.Linq;
using System.Collections.ObjectModel;

namespace WinMilk.RTM
{
    public class RestClient
    {
        private static string ReqUrl = "http://api.rememberthemilk.com/services/rest/";
        private static string AuthUrl = "http://api.rememberthemilk.com/services/auth/";

        private static string MilkApiKey = "624a977e8a0e4ce69dec0196ce6479dd";
        private static string MilkSharedKey = "ea04c412d8a8ce87";

        private string _apiKey;
        private string _sharedKey;
        private string _frob;
        private string _timeline;
        private string _token;

        private ObservableCollection<Task> _tasks;
        private ObservableCollection<string> _tags;
        private ObservableCollection<TaskList> _lists;

        public ObservableCollection<TaskList> Lists { get { return _lists; } }
        public ObservableCollection<Task> Tasks { get { return _tasks; } }
        public ObservableCollection<string> Tags { get { return _tags; } }

        private bool HasChanged { get; set; }

        public RestClient()
        {
            _apiKey = RestClient.MilkApiKey;
            _sharedKey = RestClient.MilkSharedKey;
            _token = null;

            HasChanged = true;

            LoadData();
        }

        public bool HasAuthToken
        {
            get { return _token != null; }
        }

        public void GetRequest(Dictionary<string, string> parameters, DownloadStringCompletedEventHandler callback)
        {
            parameters.Add("api_key", this._apiKey);
            if (_token != null)
            {
                parameters.Add("auth_token", _token);
            }

            parameters.Add("api_sig", this.SignParameters(parameters));

            // append params to url
            string urlWithParams = this.CreateUrl(ReqUrl, parameters);

            WebClient client = new WebClient();
            client.DownloadStringCompleted += callback;
            client.DownloadStringAsync(new Uri(urlWithParams));
        }

        public string SignParameters(Dictionary<string, string> parameters)
        {
            string sum = String.Empty;

            List<KeyValuePair<string, string>> paramList = new List<KeyValuePair<string, string>>(parameters);
            paramList.Sort((KeyValuePair<string, string> x, KeyValuePair<string, string> y) =>
            {
                return x.Key.CompareTo(y.Key);
            });


            sum += _sharedKey;
            foreach (KeyValuePair<string, string> pair in paramList)
            {
                sum += pair.Key;
                sum += pair.Value;
            }

            return JeffWilcox.Utilities.Silverlight.MD5CryptoServiceProvider.GetMd5String(sum);
        }

        public string CreateUrl(string url, Dictionary<string, string> parameters)
        {
            string urlWithParams = url + "?";

            var parArray = new string[parameters.Count];
            parameters.Keys.CopyTo(parArray, 0);
            for (int i = 0; i < parArray.Length - 1; i++)
            {
                urlWithParams += HttpUtility.UrlEncode(parArray[i]) + "=" + HttpUtility.UrlEncode(parameters[parArray[i]]) + "&";
            }
            if (parArray.Length > 0)
            {
                urlWithParams += parArray[parArray.Length - 1] + "=" + parameters[parArray[parArray.Length - 1]];
            }

            return urlWithParams;

        }

        public void GetAuthUrl(AuthUrlDelegate UrlCallback)
        {

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("method", "rtm.auth.getFrob");
            this.GetRequest(parameters, ((object sender, DownloadStringCompletedEventArgs e) =>
            {
                if (e.Error != null)
                    return;

                string frob = string.Empty;

                XElement xml = XElement.Parse(e.Result);
                IEnumerable<XElement> descendents = xml.Descendants("frob");
                frob = descendents.First().Value;

                _frob = frob;

                Dictionary<string, string> authParams = new Dictionary<string, string>();
                authParams.Add("api_key", _apiKey);
                authParams.Add("perms", "delete");
                authParams.Add("frob", frob);
                authParams.Add("api_sig", this.SignParameters(authParams));

                // append params to url
                string urlWithParams = this.CreateUrl(AuthUrl, authParams);

                UrlCallback(urlWithParams);
            }));
        }

        public void GetToken(TokenDelegate callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("method", "rtm.auth.getToken");
            parameters.Add("frob", _frob);

            this.GetRequest(parameters, ((object sender, DownloadStringCompletedEventArgs e) =>
            {
                if (e.Error != null)
                    return;

                XElement xml = XElement.Parse(e.Result);
                IEnumerable<XElement> descendents = xml.Descendants("token");
                _token = descendents.First().Value;

                SaveData();

                // after getting token but before calling back, get a new timeline
                this.GetTimeline((string timeline) =>
                {
                    HasChanged = true;
                    callback(_token);
                });
            }));
        }

        public void GetTimeline(TimelineDelegate callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("method", "rtm.timelines.create");

            this.GetRequest(parameters, ((object sender, DownloadStringCompletedEventArgs e) =>
            {
                if (e.Error != null)
                    return;

                XElement xml = XElement.Parse(e.Result);
                IEnumerable<XElement> descendents = xml.Descendants("timeline");
                _timeline = descendents.First().Value;

                SaveData();

                callback(_timeline);

            }));
        }

        public void GetAllIncompleteTasks(TasksDelegate callback, bool force, Comparison<Task> orderMethod)
        {

            if (_tasks != null && !force && !HasChanged)
            {
                callback(_tasks);
            }

            //
            // First get the list of TaskLists, so we can know names of lists
            //
            this.GetLists((ObservableCollection<TaskList> tasklists) =>
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("method", "rtm.tasks.getList");
                parameters.Add("filter", "status:incomplete");

                //
                // Then get the list of all incomplete tasks
                //
                this.GetRequest(parameters, ((object sender, DownloadStringCompletedEventArgs e) =>
                {
                    if (e.Error != null)
                    {
                        return;
                    }

                    HasChanged = false;

                    ObservableCollection<Task> list;
                    XDocument xml = XDocument.Parse(e.Result);

                    list = ParseTasks(tasklists, xml, orderMethod);

                    _tasks = list;

                    callback(list);

                }));
            }, force);
        }

        public void GetTasksFromFilter(string filter, TasksDelegate callback, Comparison<Task> orderMethod)
        {
            //
            // First get the list of TaskLists, so we can know names of lists
            //
            this.GetLists((ObservableCollection<TaskList> tasklists) =>
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("method", "rtm.tasks.getList");
                parameters.Add("filter", "status:incomplete AND " + filter);

                //
                // Then get the list of tasks according to filter
                //
                this.GetRequest(parameters, ((object sender, DownloadStringCompletedEventArgs e) =>
                {
                    if (e.Error != null)
                    {
                        return;
                    }

                    ObservableCollection<Task> list;
                    XDocument xml = XDocument.Parse(e.Result);

                    list = ParseTasks(tasklists, xml, orderMethod);

                    callback(list);

                }));
            }, false);
        }

        private static ObservableCollection<Task> ParseTasks(ObservableCollection<TaskList> tasklists, XDocument xml, Comparison<Task> orderMethod)
        {
            List<Task> list = (from element in xml.Descendants("task")
                               select new Task(
                                   int.Parse(element.Attribute("id").Value),                                        // task id
                                   int.Parse(element.Parent.Parent.Attribute("id").Value),                          // list id
                                   int.Parse(element.Parent.Attribute("id").Value),                                 // task series id
                                   element.Parent.Attribute("name").Value,		                                    // task series name
                                   element.Parent.Descendants("tag").Select(node => node.Value).ToList<string>(),	// task series tags
                                   (from note in element.Parent.Descendants("note")
                                    select new Note
                                    {
                                        Id = int.Parse(note.Attribute("id").Value),
                                        Title = note.Attribute("title").Value,
                                        Body = note.Value
                                    }
                                    ).ToList(),                                                                     // task series notes
                                   Task.StringToPriority(element.Attribute("priority").Value),					    // task priority
                                   (from tasklist in tasklists
                                    where tasklist.Id == int.Parse(element.Parent.Parent.Attribute("id").Value)
                                    select tasklist.Name
                                    ).First(),																	    // list name
                                   element.Parent.Attribute("url").Value,                                           // task series url
                                   element.Attribute("estimate").Value,                                             // task time estimate
                                   element.Attribute("has_due_time").Value == "0" ? false : true,				    // if task has due time
                                   element.Attribute("due").Value												    // task due date, in string ISO 8601 format
                                 )
                                ).ToList();

            list.Sort(orderMethod);

            ObservableCollection<Task> tasks = new ObservableCollection<Task>();
            foreach (Task t in list)
            {
                tasks.Add(t);
            }

            return tasks;
        }

        public void GetTasksDueOn(DateTime day, TasksDelegate callback)
        {
            GetAllIncompleteTasks((ObservableCollection<Task> tasks) =>
            {
                IEnumerable<Task> dueDayList = (from task in tasks
                                                where task.Due.Date == day.Date
                                                select task);

                ObservableCollection<Task> dueDay = new ObservableCollection<Task>();
                foreach (Task t in dueDayList)
                {
                    dueDay.Add(t);
                }

                callback(dueDay);
            }, false, Task.CompareByDate);
        }

        public void GetTasksDueOnOrBefore(DateTime day, TasksDelegate callback)
        {
            GetAllIncompleteTasks((ObservableCollection<Task> tasks) =>
            {
                IEnumerable<Task> dueDayList = (from task in tasks
                                                where task.Due.Date <= day.Date
                                                select task);

                ObservableCollection<Task> dueDay = new ObservableCollection<Task>();
                foreach (Task t in dueDayList)
                {
                    dueDay.Add(t);
                }

                callback(dueDay);
            }, false, Task.CompareByDate);
        }

        public void GetTask(int id, TaskDelegate callback)
        {
            GetAllIncompleteTasks((ObservableCollection<Task> tasks) =>
            {
                Task task = (from t in tasks
                             where t.Id == id
                             select t).First();

                callback(task);
            }, false, Task.CompareByDate);
        }

        public void PopulateTags()
        {
            if (_tags == null)
            {
                _tags = new ObservableCollection<string>();
            }

            if (_tasks != null)
            {
                foreach (Task t in _tasks)
                {
                    foreach (string tag in t.Tags)
                    {
                        if (!_tags.Contains(tag))
                        {
                            _tags.Add(tag);
                        }
                    }
                }
            }
        }


        public void GetLists(TaskListsDelegate callback, bool force)
        {
            if (_lists != null && !force && !HasChanged)
            {
                callback(_lists);
            }
            else
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("method", "rtm.lists.getList");

                this.GetRequest(parameters, ((object sender, DownloadStringCompletedEventArgs e) =>
                {
                    if (e.Error != null)
                    {
                        return;
                    }

                    HasChanged = false;

                    ObservableCollection<TaskList> list = new ObservableCollection<TaskList>();
                    XDocument xml = XDocument.Parse(e.Result);

                    List<TaskList> listEn =
                        (from element in xml.Descendants("list")
                         where element.Attribute("archived").Value != "1"
                         select new TaskList(
                             int.Parse(element.Attribute("id").Value),
                             element.Attribute("name").Value,
                             element.Attribute("smart").Value == "1",
                             element.Attribute("smart").Value == "1" ? element.Descendants("filter").First().Value : "",
                             TaskList.ParseSortOrder(element.Attribute("sort_order").Value)
                             )
                         ).ToList();

                    listEn.Sort();

                    foreach (TaskList l in listEn)
                    {
                        list.Add(l);
                    }

                    _lists = list;

                    callback(list);
                }));
            }
        }

        public void GetList(int id, TaskListDelegate callback)
        {
            this.GetLists((ObservableCollection<TaskList> lists) =>
            {

                TaskList list = (from TaskList l in lists
                                 where l.Id == id
                                 select l).First();

                callback(list);

            }, false);
        }

        public void GetTasksInList(TaskList list, TasksDelegate callback, bool force)
        {
            if (list.Tasks == null || list.Tasks.Count == 0 || force || HasChanged)
            {

                Comparison<Task> orderMethod;
                switch (list.SortOrder)
                {
                    case TaskListSortOrder.Priority:
                        orderMethod = Task.CompareByPriority;
                        break;
                    case TaskListSortOrder.Date:
                        orderMethod = Task.CompareByDate;
                        break;
                    case TaskListSortOrder.Name:
                        orderMethod = Task.CompareByName;
                        break;
                    default:
                        orderMethod = Task.CompareByDate;
                        break;
                }

                if (!list.IsSmart)
                {
                    GetAllIncompleteTasks((ObservableCollection<Task> tasks) =>
                    {
                        list.Tasks = new ObservableCollection<Task>();
                        IEnumerable<Task> lTasks = (from task in tasks
                                                    where task.ListId == list.Id
                                                    select task);


                        foreach (Task t in lTasks)
                        {
                            list.Tasks.Add(t);
                        }

                        callback(list.Tasks);
                    }, force, orderMethod);
                }
                else
                {
                    GetTasksFromFilter(list.Filter, (ObservableCollection<Task> tasks) =>
                    {
                        list.Tasks = tasks;

                        callback(list.Tasks);
                    }, orderMethod);
                }
            }
            else
            {
                callback(list.Tasks);
            }
        }

        public void GetTasksInListsInOrder(ObservableCollection<TaskList> lists, int position, TaskListsDelegate callback)
        {
            if (position < lists.Count)
            {
                GetTasksInList(lists[position], (t) =>
                {
                    GetTasksInListsInOrder(lists, position + 1, callback);
                }, false);
            }
            else
            {
                callback(lists);
            }
        }

        public void AddTaskWithSmartAdd(string name, TaskModifiedDelegate callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("timeline", _timeline);
            parameters.Add("method", "rtm.tasks.add");
            parameters.Add("parse", "1");
            parameters.Add("name", name);

            this.GetRequest(parameters, (object sender, DownloadStringCompletedEventArgs e) =>
            {
                if (e.Error != null)
                {
                    return;
                }

                HasChanged = true;

                callback();
            });
        }

        public void CompleteTask(Task task, TaskModifiedDelegate callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("timeline", _timeline);
            parameters.Add("method", "rtm.tasks.complete");
            parameters.Add("task_id", task.Id.ToString());
            parameters.Add("taskseries_id", task.TaskSeriesId.ToString());
            parameters.Add("list_id", task.ListId.ToString());

            this.GetRequest(parameters, (object sender, DownloadStringCompletedEventArgs e) =>
            {
                if (e.Error != null)
                {
                    return;
                }

                HasChanged = true;

                callback();
            });
        }

        public void PostponeTask(Task task, TaskModifiedDelegate callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("timeline", _timeline);
            parameters.Add("method", "rtm.tasks.postpone");
            parameters.Add("task_id", task.Id.ToString());
            parameters.Add("taskseries_id", task.TaskSeriesId.ToString());
            parameters.Add("list_id", task.ListId.ToString());

            this.GetRequest(parameters, (object sender, DownloadStringCompletedEventArgs e) =>
            {
                if (e.Error != null)
                {
                    return;
                }

                HasChanged = true;

                callback();
            });
        }

        public void DeleteTask(Task task, TaskModifiedDelegate callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("timeline", _timeline);
            parameters.Add("method", "rtm.tasks.delete");
            parameters.Add("task_id", task.Id.ToString());
            parameters.Add("taskseries_id", task.TaskSeriesId.ToString());
            parameters.Add("list_id", task.ListId.ToString());

            this.GetRequest(parameters, (object sender, DownloadStringCompletedEventArgs e) =>
            {
                if (e.Error != null)
                {
                    return;
                }

                HasChanged = true;

                callback();
            });
        }

        public void SaveData()
        {
            Helper.IsolatedStorageHelper.SaveObject<string>("token", _token);
            Helper.IsolatedStorageHelper.SaveObject<ObservableCollection<Task>>("tasks", _tasks);
            Helper.IsolatedStorageHelper.SaveObject<ObservableCollection<TaskList>>("lists", _lists);
            Helper.IsolatedStorageHelper.SaveObject<ObservableCollection<string>>("tags", _tags);
            Helper.IsolatedStorageHelper.SaveObject<string>("timeline", _timeline);
        }

        public void LoadData()
        {
            _token = Helper.IsolatedStorageHelper.GetObject<string>("token");
            _tasks = Helper.IsolatedStorageHelper.GetObject<ObservableCollection<Task>>("tasks");
            _lists = Helper.IsolatedStorageHelper.GetObject<ObservableCollection<TaskList>>("lists");
            _tags = Helper.IsolatedStorageHelper.GetObject<ObservableCollection<string>>("tags");
            _timeline = Helper.IsolatedStorageHelper.GetObject<string>("timeline");
        }

        public void DeleteData()
        {
            Helper.IsolatedStorageHelper.DeleteObject("token");
            Helper.IsolatedStorageHelper.DeleteObject("tasks");
            Helper.IsolatedStorageHelper.DeleteObject("lists");
            Helper.IsolatedStorageHelper.DeleteObject("tags");
            Helper.IsolatedStorageHelper.DeleteObject("timeline");
            LoadData();
        }
    }

    public delegate void AuthUrlDelegate(string url);
    public delegate void TokenDelegate(string token);
    public delegate void TimelineDelegate(string timeline);
    public delegate void TaskDelegate(Task task);
    public delegate void TaskListDelegate(TaskList list);
    public delegate void TasksDelegate(ObservableCollection<Task> tasks);
    public delegate void TaskListsDelegate(ObservableCollection<TaskList> lists);
    public delegate void TaskModifiedDelegate();
}
