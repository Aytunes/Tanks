using CryEngine;

namespace CryGameCode.Telemetry.Implementations
{
	public class ConsoleTelemetrySender : ITelemetrySender
	{
		static int telem_debug = 0;

		public ConsoleTelemetrySender()
		{
			CVar.RegisterInt("telem_debug", ref telem_debug, "Toggles console output of telemetry data", CVarFlags.Cheat); 
		}

		public void Send(string category, string data)
		{
			if (telem_debug != 0)
				Debug.LogAlways("[Telemetry] {0}: {1}", category, data);
		}
	}
}
