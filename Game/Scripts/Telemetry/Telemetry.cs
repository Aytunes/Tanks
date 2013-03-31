using System.Reflection;
using System.Linq;
using CryEngine;
using CryEngine.Extensions;
using System;

namespace CryGameCode.Telemetry
{
	public static class Metrics
	{
		public static TelemetryReceiver<KillData> Kills { get; private set; }

		static Metrics()
		{
			var senders = from type in Assembly.GetExecutingAssembly().GetTypes()
						  where type.Implements<ITelemetrySender>()
						  select (ITelemetrySender)Activator.CreateInstance(type);

			Debug.LogAlways("Registered {0} telemetry senders", senders.Count());

			foreach (var sender in senders)
				Debug.LogAlways(sender.GetType().Name);
			
			Kills = new TelemetryReceiver<KillData>(senders);
		}
	}

	public interface ITelemetrySender
	{
		void Send(string category, string data);
	}

	public interface ITelemetryData
	{
		string Serialize();
	}
}
