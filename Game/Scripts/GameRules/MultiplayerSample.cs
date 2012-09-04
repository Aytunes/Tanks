using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CryEngine;

using CryGameCode.Entities;

namespace CryGameCode
{
	/// <summary>
	/// Sample game mode illustrating multiplayer functionality
	/// </summary>
	public class MultiplayerSample : SinglePlayer
	{
		public override void OnClientEnteredGame(int channelId, EntityId playerId, bool reset, bool loadingSaveGame)
		{
			base.OnClientEnteredGame(channelId, playerId, reset, loadingSaveGame);

			Network.RemoteInvocation(OtherClientEnteredGame, NetworkTarget.ToOtherClients, channelId, playerId);
		}

		/// <summary>
		/// Called when another client enters the game.
		/// </summary>
		/// <param name="test"></param>
		[RemoteInvocation]
		public void OtherClientEnteredGame(int channelId, EntityId playerId)
		{
			Debug.LogAlways("Other client with channelId {0} and playerId {1} entered the game", channelId, playerId);
		}
	}
}