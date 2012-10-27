using CryEngine;

namespace CryGameCode.Tanks
{
	[Entity(Category = "Tanks")]
	public class ChaingunTank : TankTurret
	{
		public ChaingunTank(Tank tank) : base(tank) { }

		public override string Model { get { return "objects/tanks/turret_chaingun.chr"; } }
		public override bool AutomaticFire { get { return true; } }
		public override float TimeBetweenShots { get { return 0.05f; } }
	}
}
