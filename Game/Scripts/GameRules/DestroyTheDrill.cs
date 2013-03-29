using System.Collections.Generic;
using System.Linq;
using CryEngine;
using CryGameCode.Entities.Buildings;

namespace CryGameCode
{
	public class DestroyTheDrill : SinglePlayer
	{
		private Dictionary<string, Drill> m_drills;

		public override void OnClientConnect(int channelId, bool isReset = false, string playerName = "")
		{
			if (!Game.IsServer)
				return;

			// Lazy retrieval for gamerules entities
			if (m_drills == null)
			{
				m_drills = new Dictionary<string, Drill>();
				var drills = Entity.GetByClass<Drill>();

				foreach (var team in Teams)
				{
					var drill = drills.Single(d => d.Team == team);
					m_drills.Add(team, drill);
					Debug.LogAlways("Found {0}'s drill at {1}", team, drill.Position);
				}
			}

			base.OnClientConnect(channelId, isReset, playerName);
		}

		public override string[] Teams
		{
			get
			{
				return new[] { "red", "blue" };
			}
		}
	}
}
