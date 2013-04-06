using System;
using CryEngine;
using CryGameCode.Projectiles;

namespace CryGameCode.Tanks
{
	public class MG : TankTurret
	{
		public MG(Tank tank) : base(tank) { }

		protected override void OnFire(Vec3 firePos)
		{
		}

		public override string Model { get { return "objects/tanks/turret_mg.chr"; } }
		public override string LeftHelper { get { return "turret_term_2"; } }
		public override string RightHelper { get { return "turret_term_1"; } }
		public override Type ProjectileType { get { return typeof(MGBullet); } }
		public override float TimeBetweenShots { get { return 0.1f; } }
		public override bool AutomaticFire { get { return true; } }

		public override string FireSound { get { return "Sounds/weapons:hmg_fire:fire_loop_3p"; } }
	}
}