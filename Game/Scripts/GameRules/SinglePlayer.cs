using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CryEngine;
using CryEngine.Extensions;

using CryGameCode.Entities;
using CryGameCode.Tanks;
using CryGameCode.Extensions;
using CryGameCode.Network;

namespace CryGameCode
{
	/// <summary>
	/// Sample game mode illustrating multiplayer functionality
	/// </summary>
	[GameRules(Default = true)]
	public class SinglePlayer : GameRulesNativeCallbacks
	{
		private Dictionary<Type, GameRulesExtension> m_extensions;

		public SinglePlayer()
		{
			m_extensions = (from type in Assembly.GetExecutingAssembly().GetTypes()
							where type.Implements<GameRulesExtension>()
							select (GameRulesExtension)Entity.Spawn("Extension", type))
							.ToDictionary(e => e.GetType(), e => e);

			foreach (var extension in m_extensions.Values)
				extension.Register(this);

			if (Game.IsServer)
				Metrics.Record(new Telemetry.MatchStarted { GameRules = GetType().Name });

			ReceiveUpdates = true;
		}

		public T GetExtension<T>() where T : GameRulesExtension
		{
			return (T)m_extensions[typeof(T)];
		}

		public static Random Selector = new Random();
		private List<Tank> m_playerBuffer = new List<Tank>();

		public IEnumerable<Tank> Players { get { return m_playerBuffer; } }

		public override void OnClientConnect(int channelId, bool isReset = false, string playerName = "")
		{
			if (!Game.IsServer)
				return;

			Metrics.Record(new Telemetry.ClientConnected { Nickname = playerName });

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

			tank.OnDeath -= OnTankDied;

			ClientDisconnected.Raise(this, new ConnectionEventArgs { Tank = tank, ChannelID = channelId });
			Metrics.Record(new Telemetry.ClientDisconnected { Nickname = tank.Name });

			m_playerBuffer.Remove(tank);

			Actor.Remove(channelId);
		}

		public override void OnClientEnteredGame(int channelId, EntityId playerId, bool reset, bool loadingSaveGame)
		{
			Debug.LogAlways("[Enter] SinglePlayer.OnClientEnteredGame: channel {0}, player {1}", channelId, playerId);
			var actor = Actor.Get<Tank>(playerId);

			ClientConnected.Raise(this, new ConnectionEventArgs { ChannelID = channelId, Tank = actor });

			actor.OnDeath += OnTankDied;

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

		protected void OnTankDied(object sender, DamageEventArgs e)
		{
			TankDied.Raise(sender, e);
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

				var spawnPoint = FindSpawnPoint(team);
				if (spawnPoint != null)
					spawnPoint.TrySpawn(tank);

				var turretEntity = Entity.Spawn<TurretEntity>(tank.Name + "." + turretTypeName, null, null, null, true, EntityFlags.CastShadow);

				tank.OnRevived();

				// TODO: Do this on the remote client too.
				// Not possible to send its Id via the OnRevivedPlayer RMI due to the entity not having spawned on remote clients at that point.
				tank.Turret.Initialize(turretEntity);

				Debug.LogAlways("Invoking RMI OnRevivedPlayer");
				tank.RemoteInvocation(OnRevivedPlayer, NetworkTarget.ToRemoteClients, actorId, tank.Position, tank.Rotation, team, turretTypeName);
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

		public event EventHandler<ConnectionEventArgs> ClientConnected;
		public event EventHandler<ConnectionEventArgs> ClientDisconnected;
		public event EventHandler<DamageEventArgs> TankDied;
	}

	public class ConnectionEventArgs : EventArgs
	{
		public int ChannelID { get; set; }
		public Tank Tank { get; set; }
	}

	public static class EventExtensions
	{
		public static void Raise<T>(this EventHandler<T> evt, object sender, T args) where T : EventArgs
		{
			if (evt != null)
				evt(sender, args);
		}
	}
}