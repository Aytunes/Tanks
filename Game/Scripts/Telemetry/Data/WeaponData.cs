using CryEngine;
using CryGameCode.Entities;

namespace CryGameCode.Telemetry
{
	[TelemetryData]
	public struct KillData
	{
		public Vec3 Position { get; set; }
		public DamageType DamageType { get; set; }
	}

	[TelemetryData]
	public struct WeaponFiredData
	{
		public Vec3 Position { get; set; }
		public Vec3 Rotation { get; set; }
		public string Name { get; set; }
	}
}
