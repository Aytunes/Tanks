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
			if (!Game.IsServer)
				return;

			var tank = Actor.Create<Tank>(channelId, playerName);
			if (tank == null)
			{
				Debug.Log("[SinglePlayer.OnClientConnect] Failed to create the player. Check the log for errors.");
				return;
			}

			tank.ToggleSpectatorPoint();
		}

		public override void OnClientDisconnect(int channelId)
		{
			var tank = Actor.Get<Tank>(channelId);
			tank.OnLeftGame();

			Actor.Remove(channelId);
		}

		public override void OnClientEnteredGame(int channelId, EntityId playerId, bool reset, bool loadingSaveGame)
		{
			var actor = Actor.Get<Tank>(playerId);

			actor.OnEnteredGame();
			actor.RemoteInvocation(OnEnteredGame, NetworkTarget.ToRemoteClients, channelId, playerId);
		}

		[RemoteInvocation]
		void OnEnteredGame(int channelId, EntityId playerId)
		{
			var actor = Actor.Get<Tank>(playerId);

			actor.OnEnteredGame();
		}

		/// <summary>
		/// Sent to server when a remote actor wishes to be revived, change team and / or change turret type.
		/// </summary>
		/// <param name="actorId"></param>
		/// <param name="team"></param>
		/// <param name="turretTypeName"></param>
		[RemoteInvocation]
		public void RequestRevive(EntityId actorId, string team, string turretTypeName)
		{
			if (!Game.IsServer)
				return;

			Debug.LogAlways("Received revival request");
			var tank = Actor.Get<Tank>(actorId);

			if (tank.IsDead && !tank.IsDestroyed)
			{
				if (IsTeamValid(team))
					tank.Team = team;

				tank.TurretTypeName = turretTypeName;

				Debug.LogAlways("Reviving!");

				var spawnPoint = FindSpawnPoint();
				if (spawnPoint != null)
					spawnPoint.TrySpawn(tank);

				var turretEntity = Entity.Spawn<TurretEntity>(tank.Name + "." + turretTypeName, null, null, null, true, EntityFlags.CastShadow);

				tank.OnRevived();

				// TODO: Do this on the remote client too.
				// Not possible to send its Id via the OnRevivedPlayer RMI due to the entity not having spawned on remote clients at that point.
				tank.Turret.Initialize(turretEntity);

				Debug.LogAlways("Invoking RMI OnRevivedPlayer");
				tank.RemoteInvocation(OnRevivedPlayer, NetworkTarget.ToAllClients | NetworkTarget.NoLocalCalls, actorId, tank.Position, tank.Rotation, team, turretTypeName);
			}
		}

		[RemoteInvocation]
		void OnRevivedPlayer(EntityId actorId, Vec3 position, Quat rotation, string team, string turretTypeName)
		{
			Debug.LogAlways("OnRevivedPlayer");
			var tank = Actor.Get<Tank>(actorId);

			tank.Team = team;
			tank.TurretTypeName = turretTypeName;

			tank.Position = position;
			tank.Rotation = rotation;

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
					return spawnpoints.ElementAt(Selector.Next(0, spawnpoints.Count()));
			}

			return null;
		}

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