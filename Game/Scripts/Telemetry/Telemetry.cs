using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using CryEngine;
using CryEngine.Extensions;
using CryGameCode.Extensions;
using CryGameCode.Network;
using CryGameCode.Telemetry;

namespace CryGameCode
{
	[ServerOnly]
	public class Metrics : GameRulesExtension
	{
		private Dictionary<Type, TelemetryProcessor> m_processors;
		private TelemetryWriter[] m_senders;
		private Thread m_telemThread;
		private BlockingCollection<object> m_pool;
		private static int m_debug = 1;

		public static bool DebugEnabled { get { return m_debug != 0; } }

		private static Metrics m_instance;

		protected override void Init()
		{
			NetworkValidator.Server("Metrics are server-only");

			if (m_instance != null)
				throw new InvalidOperationException("Only one telemetry instance should exist");

			m_instance = this;

			CVar.RegisterInt("telem_debug", ref m_debug, "Toggles console output of telemetry data", CVarFlags.Cheat);

			m_pool = new BlockingCollection<object>();
			var types = Assembly.GetExecutingAssembly().GetTypes();

			m_senders = (from type in types
						 where type.Implements<TelemetryWriter>()
						 select (TelemetryWriter)Activator.CreateInstance(type)).ToArray();

			Debug.LogAlways("Registered {0} telemetry senders:", m_senders.Length);

			foreach (var sender in m_senders)
				Debug.LogAlways(sender.GetType().Name);

			m_processors = (from type in types
							where type.ContainsAttribute<TelemetryDataAttribute>()
							select type).ToDictionary(t => t, t => new TelemetryProcessor(t));

			Debug.LogAlways("Registered {0} telemetry processors:", m_processors.Count);

			foreach (var processor in m_processors)
				Debug.LogAlways(processor.Value.Name);

			Debug.LogAlways("Starting dedicated telemetry thread...");

			m_telemThread = new Thread(DoWork);
			m_telemThread.Start();

			Debug.LogAlways("Telemetry thread, {0}, started successfully", m_telemThread.ManagedThreadId);
		}

		protected override bool OnRemove()
		{
			if (Game.IsServer)
			{
				m_telemThread.Abort();

				foreach (var sender in m_senders)
				{
					var disposable = sender as IDisposable;
					if (disposable != null)
						disposable.Dispose();
				}
			}

			return true;
		}

		public static void Record<T>(T info)
		{
			m_instance.m_pool.Add(info);
		}

		private void DoWork()
		{
			while (true)
				Process(m_pool.Take());
		}

		private void Process(object val)
		{
			var type = val.GetType();
			var processor = m_processors[type];
			var processedData = processor.Process(val);

			Log(type.Name);

			foreach (var sender in m_senders)
				sender.Write(processor.Name, processedData);
		}

		private void Log(string format, params object[] args)
		{
			if (DebugEnabled)
				Debug.LogAlways("[Telemetry] " + format, args);
		}
	}

	public interface TelemetryWriter
	{
		void Write(string category, string data);
	}

	public class TelemetryDataAttribute : Attribute
	{
	}
}
