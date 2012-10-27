using CryEngine;

namespace CryGameCode.Tanks
{
	[Entity(Category = "Tanks")]
	public class LaserTank : TankTurret
	{
		public LaserTank(Tank tank) : base(tank) { }

		public override string Model { get { return "objects/tanks/turret_laser.chr"; } }
	}
}
