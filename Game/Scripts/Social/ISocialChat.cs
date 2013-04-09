
namespace CryGameCode.Social
{
	public interface ISocialChat
	{
		void Send(string roomId, string message);
		string CreateRoom();
		void AddUser(ulong userId, string roomId);
		void AddGroup(ulong groupId, string roomId);
	}
}
