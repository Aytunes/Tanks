using System;
using System.IO;
using CryEngine;

namespace CryGameCode.Telemetry.Implementations
{
	public class FileTelemetryWriter : TelemetryWriter, IDisposable
	{
		private const string TelemDir = "telem";
		private StreamWriter m_stream;

		private int m_flush = 0;

		public FileTelemetryWriter()
		{
			CVar.RegisterInt("telem_flush", ref m_flush, "Flush telemetry file data immediately");

			var date = DateTime.Now.ToUniversalTime();
			var filename = string.Format("telem_{0}.log", date.ToString("dd_MM_yyyy"));

			if (!Directory.Exists(TelemDir))
				Directory.CreateDirectory(TelemDir);

			m_stream = new StreamWriter(Path.Combine(TelemDir, filename), true);

			m_stream.WriteLine();
			m_stream.WriteLine("Started at {0}", date.ToShortTimeString());
		}

		public void Write(string category, string data)
		{
			m_stream.WriteLine("[{0}] {1}", category, data);

			if (m_flush != 0)
				m_stream.Flush();
		}

		public void Dispose()
		{
			m_stream.WriteLine("Log closed normally");
			m_stream.Close();
		}
	}
}
