using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryGameCode.Network
{
	public class NetSerializationException : Exception
	{
		public NetSerializationException()
		{
		}

		public NetSerializationException(string message)
			: base(message)
		{
		}

		public NetSerializationException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
