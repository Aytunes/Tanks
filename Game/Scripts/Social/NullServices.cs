using CryEngine;

namespace CryGameCode.Social.Null
{
	public class NullChatService : NullService, ISocialChat
	{
		public NullChatService(NullAuth auth)
			: base(auth)
		{
		}

		public void Send(string roomId, string message)
		{
			Log("Outgoing chat message to {0}: {1}", roomId, message);
		}

		public string CreateRoom()
		{
			return "nullroom";
		}

		public void AddUser(ulong userId, string roomId)
		{
			Log("Adding user {0} to {1}", userId, roomId);
		}

		public void AddGroup(ulong groupId, string roomId)
		{
			Log("Adding group {0} to {1}", groupId, roomId);
		}
	}

	public class NullGroupService : NullService, ISocialGroup
	{
		private GroupInfo m_groupInfo;

		public NullGroupService(NullAuth auth)
			: base(auth)
		{
			m_groupInfo = new GroupInfo(0, 0);
		}

		public GroupInfo CurrentGroup { get { return m_groupInfo; } }
	}
}
