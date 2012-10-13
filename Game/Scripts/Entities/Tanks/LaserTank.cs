using CryEngine;

namespace CryGameCode.Tanks
{
	[Entity(Category = "Tanks")]
	public class LaserTank : Tank
	{
		public override string TurretModel { get { return "objects/tanks/turret_laser.chr"; } }
	}
}
