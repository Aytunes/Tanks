using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using CryEngine;
using System.Text;

namespace CryGameCode.Network
{
	public abstract class WebRequestEntity<T> : Entity
	{
		public void RegisterGet(string query, object[] args, Action<T> action)
		{
			RegisterGet(query + CreateQueryString(args), action);
		}

		public void RegisterGet(string query, Action<T> action)
		{
			var request = WebRequest.Create(query);
			request.Proxy = null;
			request.Method = "GET";

			InternalRegister(action, request);
		}

		public void RegisterPost(string query, object[] args, Action<T> action)
		{
			RegisterPost(query, Encoding.UTF8.GetBytes(CreateQueryString(args)), action);
		}

		public void RegisterPost(string query, byte[] postData, Action<T> action)
		{
			var request = WebRequest.Create(query);
			request.Proxy = null;
			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";
			request.ContentLength = postData.Length;

			using (var stream = request.GetRequestStream())
				stream.Write(postData, 0, postData.Length);

			InternalRegister(action, request);
		}

		private string CreateQueryString(object[] args)
		{
			var builder = new StringBuilder();

			for (var i = 0; i < args.Length - 1; i += 2)
				builder.AppendFormat("{0}={1}&", args[i].ToString().ToLower(), Uri.EscapeDataString(args[i + 1].ToString()));

			if (builder.Length > 0)
				builder.Length -= 1;

			return builder.ToString();
		}

		private void InternalRegister(Action<T> action, WebRequest request)
		{
			var responseTask = Task.Factory.FromAsync<WebResponse>(request.BeginGetResponse, request.EndGetResponse, null);

			Log("Created response task");

			var transformTask = responseTask.ContinueWith(t =>
			{
				var response = t.Result;
				Log("Response is from {0}", response.ResponseUri);

				using (var responseStream = response.GetResponseStream())
				{
					var reader = new StreamReader(responseStream);

					var data = string.Empty;

					// Take the logging path to see where potential crashes occur
					if (DebugEnabled)
					{
						string line;
						while ((line = reader.ReadLine()) != null)
						{
							Log("Reading line: {0}", line);
							data += line + Environment.NewLine;
						}
					}
					else
					{
						data = reader.ReadToEnd();
					}

					Log("Got data: {0}", data);
					return Convert(data);
				}
			});

			m_tasks.Add(transformTask, action);
			ReceiveUpdates = true;
		}

		private List<Task<T>> m_doneTasks = new List<Task<T>>();

		public override void OnUpdate()
		{
			m_doneTasks.Clear();

			var i = 0;
			foreach (var kvp in m_tasks)
			{
				var task = kvp.Key;

				Log("Processing task {0}, state is {1}", ++i, task.Status);

				if (task.IsCompleted)
				{
					Log("Task is completed");
					var action = m_tasks[task];
					Log("Action is: " + action.Method.Name);
					action(task.Result);
					Log("Action completed");
					m_doneTasks.Add(task);
				}
				else if (task.IsFaulted)
				{
					Log("Task failed:");
					Debug.LogException(task.Exception);
					m_doneTasks.Add(task);
				}
			}

			foreach (var task in m_doneTasks)
				m_tasks.Remove(task);

			if (m_tasks.Count == 0)
				ReceiveUpdates = false;
		}

		protected void Log(string message, params object[] args)
		{
			if (DebugEnabled)
				Debug.LogAlways("[WebRequest] " + message, args);
		}

		protected abstract T Convert(string data);

		private static int web_debug = 0;
		public static bool DebugEnabled { get { return web_debug != 0; } }

		static WebRequestEntity()
		{
			CVar.RegisterInt("web_debug", ref web_debug);
		}

		private Dictionary<Task<T>, Action<T>> m_tasks = new Dictionary<Task<T>, Action<T>>();
	}
}
