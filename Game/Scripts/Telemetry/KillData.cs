using CryEngine;
using CryGameCode;
using CryGameCode.Entities;

namespace CryGameCode.Telemetry
{
	public struct KillData
	{
		public Vec3 Position { get; set; }
		public DamageType DamageType { get; set; }
	}
}
