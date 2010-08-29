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

namespace PhoneMilk.RTM {
	public class RestClient {
		private static string ReqUrl = "http://api.rememberthemilk.com/services/rest/";
		private static string AuthUrl = "http://api.rememberthemilk.com/services/auth/";

		private static string MilkApiKey = "624a977e8a0e4ce69dec0196ce6479dd";
		private static string MilkSharedKey = "ea04c412d8a8ce87";

		private string ApiKey;
		private string SharedKey;
		private string Frob;

		private List<TaskList> taskLists;

		private string Token;

		public RestClient() {
			this.ApiKey = RestClient.MilkApiKey;
			this.SharedKey = RestClient.MilkSharedKey;
			this.Token = null;
		}

		public RestClient(string Token) {
			this.ApiKey = MilkApiKey;
			this.SharedKey = MilkSharedKey;
			this.Token = Token;
		}

		public RestClient(string ApiKey, string SharedKey) {
			this.ApiKey = RestClient.MilkApiKey;
			this.SharedKey = RestClient.MilkSharedKey;
			this.Token = null;
		}

		public RestClient(string ApiKey, string SharedKey, string Token) {
			this.ApiKey = ApiKey;
			this.SharedKey = SharedKey;
			this.Token = Token;
		}

		public void GetRequest(Dictionary<string, string> parameters, DownloadStringCompletedEventHandler callback) {
			parameters.Add("api_key", this.ApiKey);
			if (this.Token != null) {
				parameters.Add("auth_token", this.Token);
			}

			parameters.Add("api_sig", this.SignParameters(parameters));

			// append params to url
			string urlWithParams = this.CreateUrl(ReqUrl, parameters);

			WebClient client = new WebClient();
			client.DownloadStringCompleted += callback;
			client.DownloadStringAsync(new Uri(urlWithParams));
		}

		public string SignParameters(Dictionary<string, string> parameters) {
			string sum = String.Empty;

			List<KeyValuePair<string, string>> paramList = new List<KeyValuePair<string, string>>(parameters);
			paramList.Sort((KeyValuePair<string, string> x, KeyValuePair<string, string> y) => {
				return x.Key.CompareTo(y.Key);
			});
			

			sum += this.SharedKey;
			foreach (KeyValuePair<string, string> pair in paramList) {
				sum += pair.Key;
				sum += pair.Value;
			}

			return JeffWilcox.Utilities.Silverlight.MD5CryptoServiceProvider.GetMd5String(sum);
		}

		public string CreateUrl(string url, Dictionary<string, string> parameters) {
			string urlWithParams = url + "?";

			var parArray = new string[parameters.Count];
			parameters.Keys.CopyTo(parArray, 0);
			for (int i = 0; i < parArray.Length - 1; i++) {
				urlWithParams += parArray[i] + "=" + parameters[parArray[i]] + "&";
			}
			if (parArray.Length > 0) {
				urlWithParams += parArray[parArray.Length - 1] + "=" + parameters[parArray[parArray.Length - 1]];
			}

			return urlWithParams;

		}

		public void GetAuthUrl(AuthUrlDelegate UrlCallback) {

			Dictionary<string, string> parameters = new Dictionary<string, string>();
			parameters.Add("method", "rtm.auth.getFrob");
			this.GetRequest(parameters, ((object sender, DownloadStringCompletedEventArgs e) => {
				if (e.Error != null)
					return;

				string frob = string.Empty;

				XElement xml = XElement.Parse(e.Result);
				IEnumerable<XElement> descendents = xml.Descendants("frob");
				foreach (XElement d in descendents) {
					frob = d.Value;
				}

				this.Frob = frob;

				Dictionary<string, string> authParams = new Dictionary<string, string>();
				authParams.Add("api_key", ApiKey);
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
			Dictionary<string, string> parameters = new Dictionary<string,string>();
			parameters.Add("method", "rtm.auth.getToken");
			parameters.Add("frob", this.Frob);

			this.GetRequest(parameters, ((object sender, DownloadStringCompletedEventArgs e) => {
				if (e.Error != null)
					return;

				string token = string.Empty;

				XElement xml = XElement.Parse(e.Result);
				IEnumerable<XElement> descendents = xml.Descendants("token");
				foreach (XElement d in descendents) {
					token = d.Value;
				}

				this.Token = token;

				callback(token);

			}));
		}

		public void GetTaskList(TasksDelegate callback) 
		{
			this.GetLists((List<TaskList> tasklists) =>
			{
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters.Add("method", "rtm.tasks.getList");
				parameters.Add("filter", "status:incomplete");

				this.GetRequest(parameters, ((object sender, DownloadStringCompletedEventArgs e) =>
				{
					if (e.Error != null)
					{
						return;
					}

					List<Task> list;
					XDocument xml = XDocument.Parse(e.Result);

					list = (from element in xml.Descendants("taskseries")
							  select new Task(
								  element.Attribute("name").Value,
								  element.Descendants("tag").Select(node => node.Value).ToList<string>(),
								  element.Descendants("task").Attributes("priority").Select(n => Task.StringToPriority(n.Value)).First(),
								  (from tasklist in tasklists
									where tasklist.Id == int.Parse(element.Parent.Attribute("id").Value)
									select tasklist.Name
									).First(),
									element.Descendants("task").Attributes("has_due_time").First().Value == "0" ? false : true,
								   element.Descendants("task").Attributes("due").First().Value
								  )
							 ).ToList<Task>();

					callback(list);

				}));
			});
		}

		public void GetLists(TaskListDelegate callback)
		{
			if (this.taskLists != null)
			{
				callback(this.taskLists);
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

					this.taskLists = list;

					callback(list);
				}));
			}
		}
	}

	public delegate void AuthUrlDelegate(string url);
	public delegate void TokenDelegate(string token);
	public delegate void TasksDelegate(List<Task> list);
	public delegate void TaskListDelegate(List<TaskList> list);
}
