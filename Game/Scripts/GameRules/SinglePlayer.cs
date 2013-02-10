using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CryEngine;
using CryEngine.Extensions;

using CryGameCode.Entities;
using CryGameCode.Tanks;

namespace CryGameCode
{
	/// <summary>
	/// Sample game mode illustrating multiplayer functionality
	/// </summary>
	[GameRules(Default = true)]
	public class SinglePlayer : GameRulesNativeCallbacks
	{
		public static Random Selector = new Random();

		public override void OnClientConnect(int channelId, bool isReset = false, string playerName = "")
		{
			if(!Network.IsServer)
				return;

			var tank = Actor.Create<Tank>(channelId, playerName);
			if(tank == null)
			{
				Debug.Log("[SinglePlayer.OnClientConnect] Failed to create the player. Check the log for errors.");
				return;
			}
		}

		public override void OnClientDisconnect(int channelId)
		{
			Actor.Remove(channelId);
		}

		public override void OnClientEnteredGame(int channelId, EntityId playerId, bool reset, bool loadingSaveGame)
		{
            var actor = Actor.Get<Tank>(playerId);

            actor.OnEnteredGame();

            Actors.Add(actor);
		}

        [RemoteInvocation]
        public void RequestRevive(EntityId actorId)
        {
            if (!Network.IsServer)
                return;

            var actor = Actor.Get<Tank>(actorId);
            if (actor.IsDead && !actor.IsDestroyed)
                RevivePlayer(actorId);
        }

        public virtual void RevivePlayer(EntityId actorId)
		{
			var tank = Actor.Get<Tank>(actorId);
			if(tank == null)
			{
				Debug.Log("[SinglePlayer.OnRevive] Failed to get the player. Check the log for errors.");
				return;
			}

            Debug.LogAlways("Reviving!");

            var spawnPoint = FindSpawnPoint();
            if (spawnPoint != null)
                spawnPoint.TrySpawn(tank);

            tank.OnRevived();
		}

        protected virtual SpawnPoint FindSpawnPoint(string team = null)
        {
            var spawnpoints = Entity.GetByClass<SpawnPoint>();
            if (spawnpoints.Count() > 0)
            {
                spawnpoints = spawnpoints.Where(x =>
                    {
                        return x.CanSpawn && (team == null || x.Team == team);
                    });

                if (spawnpoints.Count() > 0)
                    return spawnpoints.ElementAt(Selector.Next(0, spawnpoints.Count() - 1));
            }

            return null;
        }

        public List<Actor> Actors = new List<Actor>();

        public virtual string[] Teams
        {
            get
            {
                return new string[] { "red" };
            }
        }

        public bool IsTeamValid(string team)
        {
            return Teams.Contains(team);
        }
	}
}