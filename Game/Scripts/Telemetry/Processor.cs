using System;
using System.Linq;
using System.Reflection;

namespace CryGameCode.Telemetry
{
	public class TelemetryProcessor
	{
		public string Name { get; private set; }
		private PropertyInfo[] m_props;

		public TelemetryProcessor(Type type)
		{
			Name = type.Name;
			m_props = type.GetProperties();
		}

		public string Process(object info)
		{
			var values = m_props.Select(p => CreateEntry(p.Name, p.GetValue(info, null))).ToList();
			var time = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
			values.Add(CreateEntry("Time", time));
			return string.Join(", ", values);
		}

		private string CreateEntry(string name, object value)
		{
			return string.Format("{0}={1}", name, Uri.EscapeDataString(value.ToString()));
		}
	}
}
