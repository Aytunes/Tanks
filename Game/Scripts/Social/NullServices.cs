using CryEngine;

namespace CryGameCode.Social.Null
{
	public class NullChatService : NullService, ISocialChat
	{
		public NullChatService(NullAuth auth)
			: base(auth)
		{
		}

		public void Send(string message)
		{
			Debug.LogAlways("Chat message from {0}: {1}", Auth.Nickname, message);
		}
	}

	public class NullGroupService : NullService, ISocialGroup
	{
		private GroupInfo m_groupInfo;

		public NullGroupService(NullAuth auth)
			: base(auth)
		{
			m_groupInfo = new GroupInfo(0);
		}

		public GroupInfo CurrentGroup { get { return m_groupInfo; } }
	}
}
