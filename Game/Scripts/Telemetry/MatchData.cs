using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryGameCode.Telemetry
{
	public struct MatchStarted
	{
		public string GameRules { get; set; }
		public int Time { get; set; }
	}
}
