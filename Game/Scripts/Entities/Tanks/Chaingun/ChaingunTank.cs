using CryEngine;

namespace CryGameCode.Tanks
{
	[Entity(Category = "Tanks")]
	public class ChaingunTank : Tank
	{
		public override string TurretModel { get { return "objects/tanks/turret_chaingun.chr"; } }
		public override bool AutomaticFire { get { return true; } }
		public override float TimeBetweenShots { get { return 0.2f; } }
	}
}
