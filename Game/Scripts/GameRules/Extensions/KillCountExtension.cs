using System.Collections.Generic;
using CryEngine;
using CryGameCode.Entities;
using CryGameCode.Tanks;

namespace CryGameCode.Extensions
{
	public class KillCountExtension : GameRulesExtension
	{
		private Dictionary<Tank, int> m_counts;

		protected override void Init()
		{
			m_counts = new Dictionary<Tank, int>();

			if (Game.IsServer)
			{
				Rules.ClientConnected += Connect;
				Rules.ClientDisconnected += Disconnect;
				Rules.TankDied += TankDied;
			}
		}

		protected override bool OnRemove()
		{
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
		}

		private void Disconnect(object sender, ConnectionEventArgs e)
		{
			m_counts.Remove(e.Tank);
		}

		[RemoteInvocation]
		private void UpdateCount(EntityId id, int count)
		{
			var killer = Entity.Get<Tank>(id);
			m_counts[killer] = count;
			Debug.LogAlways("{0} now has {1} kills", killer.Name, m_counts[killer]);
		}
	}
}
