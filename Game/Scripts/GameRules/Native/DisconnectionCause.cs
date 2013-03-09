using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryGameCode
{
	/// <summary>
	/// Used to determine the cause of a client disconnection.
	/// </summary>
	public enum DisconnectionCause
	{
		/// <summary>
		/// This cause must be first! - timeout occurred.
		/// </summary>
		Timeout = 0,

		/// <summary>
		/// Incompatible protocols.
		/// </summary>
		ProtocolError,

		/// <summary>
		/// Failed to resolve an address.
		/// </summary>
		ResolveFailed,

		/// <summary>
		/// Versions mismatch.
		/// </summary>
		VersionMismatch,

		/// <summary>
		/// Server is full.
		/// </summary>
		ServerFull,

		/// <summary>
		/// User initiated kick.
		/// </summary>
		Kicked,

		/// <summary>
		/// Teamkill ban/ admin ban.
		/// </summary>
		Banned,

		/// <summary>
		/// Context database mismatch.
		/// </summary>
		ContextCorruption,

		/// <summary>
		/// Password mismatch, cdkey bad, etc.
		/// </summary>
		AuthenticationFailed,

		/// <summary>
		/// Misc. game error.
		/// </summary>
		GameError,

		/// <summary>
		/// DX11 not found.
		/// </summary>
		NotDX11Capable,

		/// <summary>
		/// The nub has been destroyed.
		/// </summary>
		NubDestroyed,

		/// <summary>
		/// Icmp reported error.
		/// </summary>
		ICMPError,

		/// <summary>
		/// NAT negotiation error.
		/// </summary>
		NatNegError,

		/// <summary>
		/// Punk buster detected something bad.
		/// </summary>
		PunkDetected,

		/// <summary>
		/// Demo playback finished.
		/// </summary>
		DemoPlaybackFinished,

		/// <summary>
		/// Demo playback file not found.
		/// </summary>
		DemoPlaybackFileNotFound,

		/// <summary>
		/// User decided to stop playing.
		/// </summary>
		UserRequested,

		/// <summary>
		/// User should have controller connected.
		/// </summary>
		NoController,

		/// <summary>
		/// Unable to connect to server.
		/// </summary>
		CantConnect,

		/// <summary>
		/// Arbitration failed in a live arbitrated session.
		/// </summary>
		ArbitrationFailed,

		/// <summary>
		/// Failed to successfully join migrated game
		/// </summary>
		FailedToMigrateToNewHost,

		/// <summary>
		/// The session has just been deleted
		/// </summary>
		SessionDeleted,

		/// <summary>
		/// Unknown cause
		/// </summary>
		Unknown
	}
}
