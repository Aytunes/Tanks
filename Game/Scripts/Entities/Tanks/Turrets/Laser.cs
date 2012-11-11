
namespace CryGameCode.Tanks
{
	public class LaserTank : TankTurret
	{
		public LaserTank(Tank tank) : base(tank) { }
        private LaserTank() { }

		public override string Model { get { return "objects/tanks/turret_laser.chr"; } }
	}
}
