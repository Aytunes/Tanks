using CryEngine;
using CryGameCode;
using CryGameCode.Entities;

namespace CryGameCode.Telemetry
{
	public struct KillData : ITelemetryData
	{
		public Vec3 Position { get; set; }
		public DamageType DamageType { get; set; }

		public string Serialize()
		{
			return string.Format("{0}|{1}", Position, DamageType);
		}
	}
}
