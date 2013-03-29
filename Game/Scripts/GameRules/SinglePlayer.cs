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
		public SinglePlayer()
		{
			ReceiveUpdates = true;
		}

		public static Random Selector = new Random();
		private List<Tank> m_playerBuffer = new List<Tank>();

		public IEnumerable<Tank> Players { get { return m_playerBuffer; } }

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

			m_playerBuffer.Remove(tank);

			Actor.Remove(channelId);
		}

		public override void OnClientEnteredGame(int channelId, EntityId playerId, bool reset, bool loadingSaveGame)
		{
			Debug.LogAlways("[Enter] SinglePlayer.OnClientEnteredGame: channel {0}, player {1}", channelId, playerId);
			var actor = Actor.Get<Tank>(playerId);

			actor.OnEnteredGame();
			actor.RemoteInvocation(OnEnteredGame, NetworkTarget.ToRemoteClients, channelId, playerId);

			// TODO: Find a neater solution to buffered calls
			foreach (var player in m_playerBuffer)
			{
				player.RemoteInvocation(OnEnteredGame, NetworkTarget.ToClientChannel, channelId, player.Id, channelId: channelId);
				player.RemoteInvocation(OnRevivedPlayer, NetworkTarget.ToClientChannel, player.Id, player.Position, player.Rotation,
					player.Team, player.TurretTypeName, channelId: channelId);
			}

			m_playerBuffer.Add(actor);
		}

		[RemoteInvocation]
		void OnEnteredGame(int channelId, EntityId playerId)
		{
			Debug.LogAlways("[Enter] SinglePlayer.OnEnteredGame: channel {0}, player {1}", channelId, playerId);
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

		public override void OnUpdate()
		{
			var removedModifiers = new List<IGameModifier>();

			foreach (var gameModifier in m_activeGameModifiers)
			{
				if (!gameModifier.Update())
					removedModifiers.Add(gameModifier);
			}

			foreach (var gameModifier in removedModifiers)
				m_activeGameModifiers.Remove(gameModifier);
		}

		public void AddGameModifier(IGameModifier modifier)
		{
			m_activeGameModifiers.Add(modifier);
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

		List<IGameModifier> m_activeGameModifiers = new List<IGameModifier>();
	}
}