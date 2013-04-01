using System.Collections.Generic;
using System.Linq;
using CryEngine;
using CryGameCode.Entities.Buildings;
using CryGameCode.Tanks;

namespace CryGameCode
{
	public class DestroyTheDrill : SinglePlayer
	{
		private Dictionary<string, TeamData> m_teams;

		public override void OnClientEnteredGame(int channelId, EntityId playerId, bool reset, bool loadingSaveGame)
		{
			base.OnClientEnteredGame(channelId, playerId, reset, loadingSaveGame);

			// Notify late-joining players of dead drills
			foreach (var team in m_teams.Values)
			{
				var drill = team.Drill;

				if (drill.IsDead)
					drill.RemoteInvocation(OnDrillDeath, NetworkTarget.ToClientChannel, drill.Id, false, channelId: channelId);
			}
		}

		public override void OnClientConnect(int channelId, bool isReset = false, string playerName = "")
		{
			if (!Game.IsServer)
				return;

			// Lazy retrieval for gamerules-related entities
			if (m_teams == null)
			{
				m_teams = new Dictionary<string, TeamData>();
				var drills = Entity.GetByClass<Drill>();

				foreach (var team in Teams)
				{
					var drill = drills.Single(d => d.Team == team);

					drill.OnDeath += (sender, e) =>
					{
						drill.RemoteInvocation(OnDrillDeath, NetworkTarget.ToAllClients, drill.Id, true);
					};

					m_teams.Add(team, new TeamData(drill));
					Debug.LogAlways("Found {0}'s drill at {1}", team, drill.Position);
				}
			}

			base.OnClientConnect(channelId, isReset, playerName);
		}

		[RemoteInvocation]
		private void OnDrillDeath(EntityId id, bool notify)
		{
			var drill = Entity.Get<Drill>(id);

			if (notify)
			{
				var msg = string.Format("{0}'s drill was destroyed!", drill.Team);
				Debug.DrawText(msg, 3, Color.White, 5);
			}

			drill.StopAnimation(blendOutTime: 1);
		}

		public override string[] Teams
		{
			get
			{
				return new[] { "red", "blue" };
			}
		}
	}

	public class TeamData
	{
		public Drill Drill { get; private set; }
		public List<Tank> Players { get; private set; }

		public TeamData(Drill drill)
		{
			Drill = drill;
			Players = new List<Tank>();
		}
	}
}
