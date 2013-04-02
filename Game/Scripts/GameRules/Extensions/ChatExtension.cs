using CryEngine;
using CryGameCode.Network;

namespace CryGameCode.Extensions
{
	public class ChatExtension : GameRulesExtension
	{
		protected override void Init()
		{
			ConsoleCommand.Register("say", (e) =>
			{
				var message = string.Join(" ", e.Args);

				if (Game.IsClient)
					RemoteInvocation(Broadcast, NetworkTarget.ToServer,  Actor.LocalClient.Name, message);
				else
					Broadcast("Server", message);
			},
			"Sends a message", CVarFlags.None, true);
		}

		[RemoteInvocation]
		private void Broadcast(string sender, string message)
		{
			NetworkValidator.Server("Chat should go through DS");
			Receive(sender, message);
			RemoteInvocation(Receive, NetworkTarget.ToAllClients, sender, message);
		}

		[RemoteInvocation]
		private void Receive(string sender, string message)
		{
			Debug.LogAlways("[Chat] {0}: {1}", sender, message);
		}
	}
}
