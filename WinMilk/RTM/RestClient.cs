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

        private List<Task> _tasks;
        private List<string> _tags;
        private List<TaskList> _lists;

        private string _token;

        public RestClient()
        {
            _apiKey = RestClient.MilkApiKey;
            _sharedKey = RestClient.MilkSharedKey;
            _token = null;

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
                foreach (XElement d in descendents)
                {
                    frob = d.Value;
                }

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

        public void GetAllIncompleteTasks(TasksDelegate callback, bool force)
        {

            if (_tasks != null && !force)
            {
                callback(_tasks);
            }

            //
            // First get the list of TaskLists, so we can know names of lists
            //
            this.GetLists((List<TaskList> tasklists) =>
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

                    List<Task> list;
                    XDocument xml = XDocument.Parse(e.Result);

                    list = (from element in xml.Descendants("task")
                            select new Task(
                                int.Parse(element.Attribute("id").Value),                                       // task id
                                int.Parse(element.Parent.Parent.Attribute("id").Value),                         // list id
                                int.Parse(element.Parent.Attribute("id").Value),                                // task series id
                                element.Parent.Attribute("name").Value,		                                    // task series name
                                element.Parent.Descendants("tag").Select(node => node.Value).ToList<string>(),	// task series tags
                                Task.StringToPriority(element.Attribute("priority").Value),					    // task priority
                                (from tasklist in tasklists
                                 where tasklist.Id == int.Parse(element.Parent.Parent.Attribute("id").Value)
                                 select tasklist.Name
                                 ).First(),																	    // list name
                                element.Attribute("has_due_time").Value == "0" ? false : true,				    // if task has due time
                                element.Attribute("due").Value												    // task due date, in string ISO 8601 format
                              )
                            ).ToList<Task>();

                    _tasks = list;

                    callback(list);

                }));
            }, force);
        }

        public void GetTasksDueOn(DateTime day, TasksDelegate callback)
        {
            GetAllIncompleteTasks((List<Task> tasks) => 
            {
                List<Task> dueDay = (from task in tasks
                              where task.Due.Date == day.Date
                              select task)
                             .ToList<Task>();

                callback(dueDay);
            }, false);
        }

        public void GetTasksDueOnOrBefore(DateTime day, TasksDelegate callback)
        {
            GetAllIncompleteTasks((List<Task> tasks) =>
            {
                List<Task> dueDay = (from task in tasks
                              where task.Due.Date <= day.Date
                              select task)
                             .ToList<Task>();

                callback(dueDay);
            }, false);
        }

        public void GetTask(int id, TaskDelegate callback)
        {
            GetAllIncompleteTasks((List<Task> tasks) =>
            {
                Task task = (from t in tasks
                             where t.Id == id
                             select t).First();

                callback(task);
            }, false);
        }

        public void PopulateTags()
        {
            if (_tags == null)
            {
                _tags = new List<string>();
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


        public void GetLists(TaskListDelegate callback, bool force)
        {
            if (this._lists != null)
            {
                callback(this._lists);
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

                    List<TaskList> list;
                    XDocument xml = XDocument.Parse(e.Result);

                    list = (from element in xml.Descendants("list")
                            select new TaskList(
                                int.Parse(element.Attribute("id").Value),
                                element.Attribute("name").Value
                                )
                             ).ToList<TaskList>();

                    _lists = list;

                    callback(list);
                }));
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

                callback();
            });
        }

        public void SaveData()
        {
            Helper.IsolatedStorageHelper.SaveObject<string>("token", _token);
            Helper.IsolatedStorageHelper.SaveObject<List<Task>>("tasks", _tasks);
            Helper.IsolatedStorageHelper.SaveObject<List<TaskList>>("lists", _lists);
            Helper.IsolatedStorageHelper.SaveObject<List<string>>("tags", _tags);
            Helper.IsolatedStorageHelper.SaveObject<string>("timeline", _timeline);
        }

        public void LoadData()
        {
            _token = Helper.IsolatedStorageHelper.GetObject<string>("token");
            _tasks = Helper.IsolatedStorageHelper.GetObject<List<Task>>("tasks");
            _lists = Helper.IsolatedStorageHelper.GetObject<List<TaskList>>("lists");
            _tags = Helper.IsolatedStorageHelper.GetObject<List<string>>("tags");
            _token = Helper.IsolatedStorageHelper.GetObject<string>("timeline");
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
    public delegate void TasksDelegate(List<Task> list);
    public delegate void TaskListDelegate(List<TaskList> list);
    public delegate void TaskModifiedDelegate();
}
