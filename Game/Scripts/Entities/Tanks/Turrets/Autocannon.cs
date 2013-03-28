using System;
using CryEngine;
using CryGameCode.Projectiles;

namespace CryGameCode.Tanks
{
	public class Autocannon : TankTurret
	{
		public Autocannon(Tank tank) : base(tank) { }

		protected override void OnFire(Vec3 firePos)
		{
		}

		public override string Model { get { return "objects/tanks/turret_autocannon.chr"; } }
		public override Type ProjectileType { get { return typeof(AutocannonRocket); } }
		public override bool AutomaticFire { get { return true; } }
		public override float TimeBetweenShots { get { return 0.5f; } }
	}
}
