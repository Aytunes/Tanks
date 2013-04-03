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

		public string Process<T>(T info)
		{
			var values = m_props.Select(p =>
			{
				var propValue = p.GetValue(info, null).ToString();
				return string.Format("{0} = {1}", p.Name, Uri.EscapeDataString(propValue));
			});
			
			return string.Join("\t,", values);
		}
	}
}
