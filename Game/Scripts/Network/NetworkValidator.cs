using System;
using CryEngine;

namespace CryGameCode.Network
{
	public static class NetworkValidator
	{
		public static void Server(string reason = null)
		{
			if (!Game.IsServer)
				throw new NetworkValidationException(string.Format("Server access level denied: {0}", reason ?? "no reason given"));
		}
	}

	public class NetworkValidationException : Exception
	{
		public NetworkValidationException()
		{
		}

		public NetworkValidationException(string message)
			: base(message)
		{
		}

		public NetworkValidationException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}

	public class DedicatedServerOnlyAttribute : Attribute
	{
	}

	public class ServerOnlyAttribute : Attribute
	{
	}

	public class ExcludeInEditorAttribute : Attribute
	{
	}
}
