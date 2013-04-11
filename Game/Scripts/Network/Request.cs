using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CryEngine;

namespace CryGameCode.Network
{
	public static class Request
	{
		private static int web_debug = 1;
		public static bool DebugEnabled { get { return web_debug != 0; } }

		static Request()
		{
			CVar.RegisterInt("web_debug", ref web_debug);
		}

		public static Task<string> Get(string query, params object[] args)
		{
			var request = Create(query + CreateQueryString(args), "GET");
			return GetResponse(request);
		}

		public static Task<string> Post(string query, params object[] args)
		{
			return Post(query, Encoding.UTF8.GetBytes(CreateQueryString(args)));
		}

		public static Task<string> Post(string query, byte[] postData)
		{
			var request = Create(query, "POST");
			request.ContentType = "application/x-www-form-urlencoded";
			request.ContentLength = postData.Length;

			using (var stream = request.GetRequestStream())
				stream.Write(postData, 0, postData.Length);

			return GetResponse(request);
		}

		private static Task<string> GetResponse(WebRequest request)
		{
			var requestTask = Task.Factory.FromAsync<WebResponse>(request.BeginGetResponse, request.EndGetResponse, null);

			return requestTask.ContinueWith(t =>
			{
				var response = t.Result;

				using (var responseStream = response.GetResponseStream())
				{
					var reader = new StreamReader(responseStream);
					return reader.ReadToEnd();
				}
			});
		}

		private static WebRequest Create(string query, string type)
		{
			var request = WebRequest.Create(query);
			request.Proxy = null;
			request.Method = type;
			return request;
		}

		private static string CreateQueryString(object[] args)
		{
			var builder = new StringBuilder();

			for (var i = 0; i < args.Length - 1; i += 2)
				builder.AppendFormat("{0}={1}&", args[i].ToString().ToLower(), Uri.EscapeDataString(args[i + 1].ToString()));

			if (builder.Length > 0)
			{
				builder.Insert(0, "?");
				builder.Length -= 1;
			}

			return builder.ToString();
		}
	}
}
