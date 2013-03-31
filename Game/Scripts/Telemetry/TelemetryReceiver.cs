using System.Collections.Generic;
using System.Linq;

namespace CryGameCode.Telemetry
{
	public class TelemetryReceiver<T> where T : ITelemetryData
	{
		private ITelemetrySender[] m_senders;
		private string m_name;

		public TelemetryReceiver(IEnumerable<ITelemetrySender> senders)
		{
			m_name = typeof(T).Name;
			m_senders = senders.ToArray();
		}

		public void Record(T info)
		{
			var data = info.Serialize();
			foreach (var sender in m_senders)
				sender.Send(m_name, data);
		}
	}
}
