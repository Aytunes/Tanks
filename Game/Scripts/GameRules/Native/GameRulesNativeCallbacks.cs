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
		/// <summary>
		/// Called when a new client has connected to the server.
		/// </summary>
		/// <param name="channelId"></param>
		/// <param name="isReset"></param>
		/// <param name="playerName"></param>
		public virtual bool OnClientConnect(int channelId, bool isReset = false, string playerName = "Dude") { return false; }
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
		public virtual void OnClientEnteredGame(int channelId, EntityId playerId, bool reset) { }

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

		public virtual void OnCollision(EntityId sourceId, EntityId targetId, Vec3 hitPos, Vec3 dir, short materialId, Vec3 contactNormal) { }

		public virtual void OnEditorReset(bool enterGamemode) { }
	}
}
