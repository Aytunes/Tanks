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
		private static List<Type> TankTypes;
		private static Random Selector = new Random();

		static SinglePlayer()
		{
			TankTypes = (from type in Assembly.GetExecutingAssembly().GetTypes()
						 where type.Implements<Tank>()
						 select type).ToList();

			ConsoleCommand.Register("SetTankType", (e) =>
			{
				ForceTankType = e.Args[0];
			});

			ConsoleCommand.Register("ResetTankType", (e) =>
			{
				ForceTankType = string.Empty;
			});
		}

		private static string ForceTankType;

		public override void OnClientConnect(int channelId, bool isReset = false, string playerName = "")
		{
			if (!Network.IsServer)
				return;

			Type tankType;

			if (string.IsNullOrEmpty(ForceTankType))
				tankType = TankTypes[Selector.Next(TankTypes.Count)];
			else
				tankType = Type.GetType("CryGameCode.Tanks." + ForceTankType, true, true);

			var player = Actor.Create(tankType, channelId, playerName);
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
			if (Network.IsEditor)
				RevivePlayer(actorId);
		}

		public override void OnClientEnteredGame(int channelId, EntityId playerId, bool reset, bool loadingSaveGame)
		{
			if (!Network.IsEditor)
				RevivePlayer(playerId);
		}

		void RevivePlayer(EntityId actorId)
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

			var tank2 = Actor.Create<HeavyTank>(2);
		}

		public bool IsTeamValid(string team)
		{
			return Teams.Contains(team);
		}
	}
}