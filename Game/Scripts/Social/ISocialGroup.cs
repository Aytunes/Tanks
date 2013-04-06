
namespace CryGameCode.Social
{
	public interface ISocialGroup
	{
		GroupInfo CurrentGroup { get; }
	}

	public class GroupInfo
	{
		public ulong LeaderID { get; private set; }
		public ulong[] UserIDs { get; private set; }

		public GroupInfo(ulong leaderId, params ulong[] users)
		{
			LeaderID = leaderId;
			UserIDs = users;
		}
	}
}
