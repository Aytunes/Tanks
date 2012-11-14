using System.Linq;

using CryEngine;

using CryGameCode.Tanks;
using CryGameCode.Entities;

namespace CryGameCode
{
	public class DeathMatch : SinglePlayer
	{
        public override void RevivePlayer(EntityId actorId)
        {
            var tank = Actor.Get<Tank>(actorId);
            if (tank == null)
            {
                Debug.Log("[SinglePlayer.OnRevive] Failed to get the player. Check the log for errors.");
                return;
            }

            var spawnpoints = Entity.GetByClass<SpawnPoint>();
            if (spawnpoints.Count() > 0)
            {
                var spawnPoint = spawnpoints.ElementAt(Selector.Next(0, spawnpoints.Count() - 1));

                spawnPoint.TrySpawn(tank);
            }

            tank.OnRevive();
        }
    }
}
