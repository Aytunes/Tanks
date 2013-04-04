using System.Collections.Generic;
using System.Linq;
using CryEngine;
using CryGameCode.Entities;
using CryGameCode.Tanks;

namespace CryGameCode.Extensions
{
	public class KillCountExtension : GameRulesExtension
	{
		private Dictionary<Tank, int> m_counts;
		private bool m_viewData;

		protected override void Init()
		{
			m_counts = new Dictionary<Tank, int>();

			if (Game.IsServer)
			{
				Rules.ClientConnected += Connect;
				Rules.ClientDisconnected += Disconnect;
				Rules.TankDied += TankDied;
			}
			
			if (Game.IsClient)
			{
				ReceiveUpdates = true;

				Input.ActionmapEvents.Add("ui_down", (e) =>
				{
					if (e.KeyEvent == KeyEvent.OnPress)
						m_viewData = true;
					else if (e.KeyEvent == KeyEvent.OnRelease)
						m_viewData = false;
				});
			}
		}

		public override void OnUpdate()
		{
			if (m_viewData)
			{
				var height = 100;
				Renderer.DrawTextToScreen(10, height, 2, Color.White, "{0} players:", m_counts.Count);

				foreach (var kvp in m_counts)
				{
					height += 20;
					Renderer.DrawTextToScreen(10, height, 2, Color.White, "{0}: {1}", kvp.Key.Name, kvp.Value);
				}
			}
		}

		protected override bool OnRemove()
		{
			Input.ActionmapEvents.RemoveAll(this);
			Rules.ClientConnected -= Connect;
			Rules.ClientDisconnected -= Disconnect;
			Rules.TankDied -= TankDied;
			return true;
		}

		private void TankDied(object sender, DamageEventArgs e)
		{
			var killer = Entity.Get<Tank>(e.Source);

			if (killer == null)
				return;

			m_counts[killer]++;

			UpdateCount(killer.Id, m_counts[killer]);
			RemoteInvocation(UpdateCount, NetworkTarget.ToAllClients, killer.Id, m_counts[killer]);
		}

		private void Connect(object sender, ConnectionEventArgs e)
		{
			m_counts.Add(e.Tank, 0);

			// Get state for existing clients
			foreach (var player in Rules.Players)
				RemoteInvocation(UpdateCount, NetworkTarget.ToClientChannel, player.Id, m_counts[player], channelId: e.ChannelID);

			RemoteInvocation(UpdateCount, NetworkTarget.ToAllClients, e.Tank.Id, 0);
		}

		private void Disconnect(object sender, ConnectionEventArgs e)
		{
			m_counts.Remove(e.Tank);
			RemoteInvocation(NotifyCleanup, NetworkTarget.ToAllClients);
		}

		[RemoteInvocation]
		private void UpdateCount(EntityId id, int count)
		{
			var killer = Entity.Get<Tank>(id);

			if (!m_counts.ContainsKey(killer))
				m_counts.Add(killer, count);
			else
				m_counts[killer] = count;
			
			Debug.LogAlways("{0} now has {1} kills", killer.Name, count);
		}

		[RemoteInvocation]
		private void NotifyCleanup()
		{
			var toRemove = m_counts.Keys.Where(t => t.IsDestroyed).ToArray();
			foreach (var tank in toRemove)
				m_counts.Remove(tank);
		}
	}
}
