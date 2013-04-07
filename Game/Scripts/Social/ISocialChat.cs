
namespace CryGameCode.Social
{
	public interface ISocialChat
	{
		void Send(string roomId, string message);
		string CreateRoom();
	}
}
