using CryEngine;

namespace CryGameCode
{
	/// <summary>
	/// All game rules callbacks contained in CryGame.dll should be listed here.
	/// </summary>
	[ExcludeFromCompilation]
	public class GameRulesNativeCallbacks : GameRules
	{
		// Shared
		public virtual void PrecacheLevel() { }
		public virtual void RequestSpawnGroup(EntityId spawnGroupId) { }
		public virtual void SetPlayerSpawnGroup(EntityId playerId, EntityId spawnGroupId) { }
		public virtual EntityId GetPlayerSpawnGroup(EntityId actorId) { return new EntityId(System.Convert.ToUInt32(0)); }
		public virtual void ShowScores(bool show) { }

        public virtual void OnSetTeam(EntityId actorId, int teamId) { }

        /// <summary>
        /// Called when a new client has connected to the server.
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="isReset"></param>
        /// <param name="playerName"></param>
		public virtual void OnClientConnect(int channelId, bool isReset = false, string playerName = "Dude") { }
		/// <summary>
		/// Called when the client disconnects from the server.
		/// </summary>
		/// <param name="channelId"></param>
        public virtual void OnClientDisconnect(int channelId) { }

        /// <summary>
        /// Called after <see cref="OnClientConnect"/>, when the new client has successfully loaded the level.
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="playerId"></param>
        /// <param name="reset"></param>
        /// <param name="loadingSaveGame"></param>
		public virtual void OnClientEnteredGame(int channelId, EntityId playerId, bool reset, bool loadingSaveGame) { }

		public virtual void OnChangeTeam(EntityId actorId, int teamId) { }

		public virtual void RestartGame(bool forceInGame) { }

		// Client-only
        /// <summary>
        /// Called on the local client when connecting to a new server.
        /// </summary>
		public virtual void OnConnect() { }
        /// <summary>
        /// Called on the local client when disconnecting from the server.
        /// </summary>
        /// <param name="cause"></param>
        /// <param name="description"></param>
		public virtual void OnDisconnect(DisconnectionCause cause, string description) { }

        /// <summary>
        /// Called when the local client has connected to a new server server, following <see cref="OnConnect"/>.
        /// </summary>
        public virtual void OnConnected(EntityId id) { }
        
        public virtual void OnRevive(EntityId actorId, Vec3 pos, Vec3 rot, int teamId) { }
		public virtual void OnReviveInVehicle(EntityId actorId, EntityId vehicleId, int seatId, int teamId) { }
		public virtual void OnKill(EntityId actorId, EntityId shooterId, string weaponClassName, int damage, int material, int hitType) { }

		public virtual void OnCollision(EntityId sourceId, EntityId targetId, Vec3 hitPos, Vec3 dir, short materialId, Vec3 contactNormal) { }

        /// <summary>
        /// Sent to all clients when a new player has entered the game.
        /// </summary>
        /// <param name="playerName"></param>
        /// <param name="playerId"></param>
        public virtual void OnPlayerJoined(string playerName, EntityId playerId) { }
        public virtual void OnPlayerLeft(string playerName, EntityId playerId) { }
    }
}
