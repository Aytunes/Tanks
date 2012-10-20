using System.Linq;

using CryEngine;

using CryGameCode.Tanks;
using CryGameCode.Entities;

namespace CryGameCode
{
	public class DeathMatch : SinglePlayer
	{
		public override void OnRevive(EntityId actorId, Vec3 pos, Vec3 rot, int teamId)
		{
			var player = Actor.Get<Tank>(actorId);
			if (player == null)
			{
				Debug.Log("[SinglePlayer.OnRevive] Failed to get the player. Check the log for errors.");
				return;
			}

			var spawnpoints = Entity.GetByClass<SpawnPoint>();
			Debug.LogAlways("Found {0} spawns_", spawnpoints.Count());
			if (spawnpoints.Count() > 0)
			{
				var spawnpoint = spawnpoints.First();

				player.Position = spawnpoint.Position;
				player.Rotation = spawnpoint.Rotation;
			}
		}
	}
}
