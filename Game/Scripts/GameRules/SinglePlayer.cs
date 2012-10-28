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
		public static string[] Teams = { "red", "blue" };
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

		public override void OnRevive(EntityId actorId, Vec3 pos, Vec3 rot, int teamId)
		{
			if(Network.IsEditor)
				RevivePlayer(actorId);
		}

		public override void OnClientEnteredGame(int channelId, EntityId playerId, bool reset, bool loadingSaveGame)
		{
			if(!Network.IsEditor)
				RevivePlayer(playerId);
		}

		void RevivePlayer(EntityId actorId)
		{
			var tank = Actor.Get<Tank>(actorId);
			if(tank == null)
			{
				Debug.Log("[SinglePlayer.OnRevive] Failed to get the player. Check the log for errors.");
				return;
			}

			var spawnpoints = Entity.GetByClass<SpawnPoint>();
			if(spawnpoints.Count() > 0)
			{
				var spawnPoint = spawnpoints.ElementAt(Selector.Next(0, spawnpoints.Count() - 1));

				spawnPoint.TrySpawn(tank);
			}
		}

		public bool IsTeamValid(string team)
		{
			return Teams.Contains(team);
		}
	}
}