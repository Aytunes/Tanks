using System;
using CryGameCode.Projectiles;

namespace CryGameCode.Tanks
{
	public class Rocket : TankTurret
	{
		public Rocket(Tank tank) : base(tank) { }
        private Rocket() { }

		public override string Model { get { return "objects/tanks/turret_rocket.chr"; } }
		public override string LeftHelper { get { return "turret_term_2"; } }
		public override string RightHelper { get { return "turret_term_1"; } }
		public override float TimeBetweenShots { get { return 0.3f; } }
		public override Type ProjectileType { get { return typeof(Rocket); } }
	}
}
