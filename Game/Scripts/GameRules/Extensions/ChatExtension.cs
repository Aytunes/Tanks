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
		private ISocialGroup m_group;

		private string m_roomName;

		protected override void Init()
		{
			var platform = SocialPlatform.Active;
			m_chat = platform.Chat;
			m_group = platform.Group;

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
				RequestRoomUpdate();
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

		private void RequestRoomUpdate()
		{
			var platform = SocialPlatform.Active;

			var currentGroup = m_group.CurrentGroup;
			var groupId = currentGroup.ID;
			var user = platform.CurrentUser;

			RemoteInvocation(RequestRoomName, NetworkTarget.ToServer, string.Empty, groupId.ToString(), user.ToString());
		}

		private float m_lastRequest;
		private const float m_requestTime = 1;

		public override void OnUpdate()
		{
			m_lastRequest += Time.DeltaTime;

			if (m_roomName == null && m_lastRequest > m_requestTime)
			{
				m_lastRequest = 0;
				var platform = SocialPlatform.Active;
				RequestRoomUpdate();
			}

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

		private List<ulong> m_userIds = new List<ulong>();
		private List<ulong> m_groupIds = new List<ulong>();

		[RemoteInvocation]
		private void RequestRoomName(string dummy, string userId, string groupId)
		{
			NetworkValidator.Server("Server owns room name");

			Debug.LogAlways("Room name request: {0} from {1}", userId, groupId);

			var group = ulong.Parse(groupId);
			var user = ulong.Parse(userId);

			if (!m_userIds.Contains(user))
			{
				m_userIds.Add(user);
				m_chat.AddUser(user, m_roomName);
				UpdateRemoteRoom();
			}

			if (!m_groupIds.Contains(group))
			{
				m_groupIds.Add(group);
				m_chat.AddGroup(group, m_roomName);
				UpdateRemoteRoom();
			}
		}

		private void UpdateRemoteRoom()
		{
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
