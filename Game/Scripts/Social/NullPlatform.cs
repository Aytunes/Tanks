using CryEngine;
using System.Collections.Generic;

namespace CryGameCode.Social.Null
{
	public class NullPlatform : ISocialPlatform
	{
		public ISocialChat Chat { get; private set; }
		public ISocialGroup Group { get; private set; }

		public void Init(Dictionary<string, object> args)
		{
			var nickname = (string)args["nickname"];
			var auth = new NullAuth(nickname);

			Debug.LogAlways("[NullPlatform] Logging in as {0}", nickname);

			Chat = new NullChatService(auth);
			Group = new NullGroupService(auth);
		}
	}

	public abstract class NullService
	{
		protected NullAuth Auth { get; private set; }

		public NullService(NullAuth auth)
		{
			Auth = auth;
		}
	}

	public class NullAuth
	{
		public string Nickname { get; private set; }

		public NullAuth(string nickname)
		{
			Nickname = nickname;
		}
	}
}
