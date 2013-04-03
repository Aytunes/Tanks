
namespace CryGameCode.Telemetry
{
	[TelemetryData]
	public struct MatchStarted
	{
		public string GameRules { get; set; }
	}

	[TelemetryData]
	public struct ClientConnected
	{
		public string Nickname { get; set; }
	}

	[TelemetryData]
	public struct ClientDisconnected
	{
		public string Nickname { get; set; }
	}
}
