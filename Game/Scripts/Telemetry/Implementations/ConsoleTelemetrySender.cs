using CryEngine;

namespace CryGameCode.Telemetry.Implementations
{
	public class ConsoleTelemetrySender : ITelemetrySender
	{
		public void Send(string category, string data)
		{
			Debug.LogAlways("[Telemetry] {0}: {1}", category, data);
		}
	}
}
