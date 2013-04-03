
namespace CryGameCode.Telemetry
{
	[TelemetryData]
	public struct MatchStarted
	{
		public string GameRules { get; set; }
		public int Time { get; set; }
	}
}
