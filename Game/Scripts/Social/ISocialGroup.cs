
namespace CryGameCode.Social
{
	public interface ISocialGroup
	{
		GroupInfo CurrentGroup { get; }
	}

	public class GroupInfo
	{
		public ulong ID { get; private set; }
		public ulong LeaderID { get; private set; }
		public ulong[] UserIDs { get; private set; }

		public GroupInfo(ulong id, ulong leaderId, params ulong[] users)
		{
			ID = id;
			LeaderID = leaderId;
			UserIDs = users;
		}
	}
}
