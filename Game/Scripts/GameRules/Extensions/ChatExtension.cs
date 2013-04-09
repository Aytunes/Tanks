using System.Collections;
using CryEngine;
using CryGameCode.Network;
using CryGameCode.Social;
using System.Collections.Generic;

namespace CryGameCode.Extensions
{
	[ExcludeInEditor]
	public class ChatExtension : GameRulesExtension
	{
		private ISocialChat m_chat;
		private string m_roomName;

		protected override void Init()
		{
			m_chat = SocialPlatform.Active.Chat;

			if (Game.IsServer)
			{
				Rules.ClientConnected += Connect;
				Rules.ClientDisconnected += Disconnect;
				m_roomName = m_chat.CreateRoom();
			}
			if (Game.IsClient)
			{
				m_messages = new Queue<string>();
				ReceiveUpdates = true;
				RemoteInvocation(RequestRoomName, NetworkTarget.ToServer);
			}

			ConsoleCommand.Register("say", (e) =>
			{
				var message = string.Join(" ", e.Args);

				m_chat.Send(m_roomName, message);

				if (Game.IsPureClient)
					RemoteInvocation(Broadcast, NetworkTarget.ToServer, Actor.LocalClient.Name, message);
				else
					Broadcast(Game.IsClient ? Actor.LocalClient.Name : "Server", message);
			},
			"Sends a message", CVarFlags.None, true);
		}

		public override void OnUpdate()
		{
			if (m_roomName == null)
				RemoteInvocation(RequestRoomName, NetworkTarget.ToServer);

			if (m_messages.Count > 0)
			{
				float y_start = 500;
				foreach (var msg in m_messages)
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
		private void RequestRoomName()
		{
			NetworkValidator.Server("Server owns room name");
			RemoteInvocation(SetRoomId, NetworkTarget.ToRemoteClients, string.Empty, m_roomName);
		}

		// TODO: Fix string RMIs
		[RemoteInvocation]
		private void SetRoomId(string dummy, string id)
		{
			Debug.LogAlways("Got new room name: {0}", id);
			m_roomName = id;
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

		private Queue<string> m_messages;
	}
}
