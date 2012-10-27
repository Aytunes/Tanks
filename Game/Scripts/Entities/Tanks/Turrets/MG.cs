using CryEngine;

namespace CryGameCode.Tanks
{
	[Entity(Category = "Tanks")]
	public class MGTank : TankTurret
	{
		public MGTank(Tank tank) : base(tank) { }

		public override string Model { get { return "objects/tanks/turret_mg.chr"; } }
		public override string LeftHelper { get { return "turret_term_2"; } }
		public override string RightHelper { get { return "turret_term_1"; } }
		public override float TimeBetweenShots { get { return 0.1f; } }
		public override bool AutomaticFire { get { return true; } }
	}
}
