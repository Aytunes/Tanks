using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CryEngine;

using CryGameCode.Tanks;
using CryGameCode.Entities;

namespace CryGameCode
{
	/// <summary>
	/// Sample game mode illustrating multiplayer functionality
	/// </summary>
	[GameRules(Default = true)]
	public class SinglePlayer : GameRulesNativeCallbacks
	{
		public override void OnClientConnect(int channelId, bool isReset = false, string playerName = "")
		{
			if (!Network.IsServer)
				return;

			var player = Actor.Create<CameraProxy>(channelId, playerName);
			if (player == null)
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
			var player = Actor.Get<CameraProxy>(actorId);
			if (player == null)
			{
				Debug.Log("[SinglePlayer.OnRevive] Failed to get the player. Check the log for errors.");
				return;
			}

			var tank = Entity.Spawn<HeavyTank>(player.Name);

			player.TargetEntity = tank;

			var spawnpoints = Entity.GetByClass<SpawnPoint>();
			foreach (var spawnpoint in spawnpoints)
			{
				if (spawnpoint.TrySpawn(tank))
					break;
			}

			player.Init();
		}
	}
}