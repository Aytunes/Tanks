using CryEngine;
using CryGameCode.Network;
using System.Collections;
using System;
using CryGameCode.Social;

namespace CryGameCode.Extensions
{
	[ExcludeInEditor]
	public class ChatExtension : GameRulesExtension
	{
		private ISocialChat m_chat;

		protected override void Init()
		{
			if (Game.IsServer)
			{
				Rules.ClientConnected += Connect;
				Rules.ClientDisconnected += Disconnect;
			}
			if (Game.IsClient)
			{
				m_messages = new Queue();
				ReceiveUpdates = true;
			}

			ConsoleCommand.Register("say", (e) =>
			{
				var message = string.Join(" ", e.Args);

				m_chat.Send(message);

				if (Game.IsPureClient)
					RemoteInvocation(Broadcast, NetworkTarget.ToServer, Actor.LocalClient.Name, message);
				else
					Broadcast(Game.IsClient ? Actor.LocalClient.Name : "Server", message);
			},
			"Sends a message", CVarFlags.None, true);
		}

		public override void OnUpdate()
		{
			if (m_messages.Count > 0)
			{
				float y_start = 500;
				foreach (string msg in m_messages)
				{
					Renderer.DrawTextToScreen(10, y_start, 1.3f, Color.White, msg);
					y_start += 10;
				}
			}
		}

		private void Connect(object sender, ConnectionEventArgs e)
		{
			Broadcast("[Join]", e.Tank.Name + " joined the game.");
		}

		private void Disconnect(object sender, ConnectionEventArgs e)
		{
			Broadcast("[Quit]", e.Tank.Name + " has left the game.");
		}

		[RemoteInvocation]
		private void Broadcast(string sender, string message)
		{
			NetworkValidator.Server("Chat should go through DS");

			if (message.Length > 64)
			{
				message = message.Substring(0, 64);
			}

			Receive(sender, message);
			RemoteInvocation(Receive, NetworkTarget.ToRemoteClients, sender, message);
		}

		[RemoteInvocation]
		private void Receive(string sender, string message)
		{
			Debug.LogAlways("[Chat] {0}: {1}", sender, message);
			if (Game.IsClient && m_messages != null)
			{
				m_messages.Enqueue(string.Format("{0}: " + message, sender));

				if (m_messages.Count > 5)
					m_messages.Dequeue();
			}
		}

		private Queue m_messages;
	}
}
