using CryGameCode.Telemetry;

namespace CryGameCode.Telemetry
{
	public static class Metrics
	{
		public static TelemetryReceiver<KillData> Kills { get; private set; }

		static Metrics()
		{
			Kills = new TelemetryReceiver<KillData>();
		}
	}
}
