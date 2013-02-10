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

		public virtual void OnClientConnect(int channelId, bool isReset = false, string playerName = "Dude") { }
		public virtual void OnClientDisconnect(int channelId) { }

		public virtual void OnClientEnteredGame(int channelId, EntityId playerId, bool reset, bool loadingSaveGame) { }

		public virtual void OnChangeTeam(EntityId actorId, int teamId) { }

		public virtual void RestartGame(bool forceInGame) { }

		// Client-only
		public virtual void OnConnect() { }
		public virtual void OnDisconnect(DisconnectionCause cause, string description) { }

		public virtual void OnRevive(EntityId actorId, Vec3 pos, Vec3 rot, int teamId) { }
		public virtual void OnReviveInVehicle(EntityId actorId, EntityId vehicleId, int seatId, int teamId) { }
		public virtual void OnKill(EntityId actorId, EntityId shooterId, string weaponClassName, int damage, int material, int hitType) { }

		public virtual void OnCollision(EntityId sourceId, EntityId targetId, Vec3 hitPos, Vec3 dir, short materialId, Vec3 contactNormal) { }
	}
}
