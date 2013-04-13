using CryEngine;
using CryGameCode.Entities;

namespace CryGameCode.Telemetry
{
	[TelemetryData]
	public struct KillData
	{
		public IntVec2 Position { get; set; }
		public DamageType DamageType { get; set; }
	}

	[TelemetryData]
	public struct WeaponFiredData
	{
		public IntVec2 Position { get; set; }
		public int Rotation { get; set; }
		public string Name { get; set; }
	}
}
