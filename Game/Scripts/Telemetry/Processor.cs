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
			var values = m_props.Select(p => string.Format("{0}: {1}", p.Name, p.GetValue(info, null).ToString()));
			return string.Join("|", values);
		}
	}
}
