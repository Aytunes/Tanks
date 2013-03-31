using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CryEngine;
using CryEngine.Extensions;
using CryGameCode.Telemetry;

namespace CryGameCode
{
	public static class Metrics
	{
		private static Dictionary<Type, TelemetryProcessor> m_processors;
		private static ITelemetrySender[] m_senders;

		static Metrics()
		{
			m_processors = new Dictionary<Type, TelemetryProcessor>();

			var senders = from type in Assembly.GetExecutingAssembly().GetTypes()
						  where type.Implements<ITelemetrySender>()
						  select (ITelemetrySender)Activator.CreateInstance(type);

			Debug.LogAlways("Registered {0} telemetry senders", senders.Count());

			foreach (var sender in senders)
				Debug.LogAlways(sender.GetType().Name);

			m_senders = senders.ToArray();
		}

		public static void Record<T>(T info)
		{
			var type = typeof(T);
			if (!m_processors.ContainsKey(type))
				m_processors.Add(type, new TelemetryProcessor(type));

			var processor = m_processors[type];
			var data = processor.Process(info);

			foreach (var sender in m_senders)
				sender.Send(processor.Name, data);
		}
	}

	public interface ITelemetrySender
	{
		void Send(string category, string data);
	}
}
