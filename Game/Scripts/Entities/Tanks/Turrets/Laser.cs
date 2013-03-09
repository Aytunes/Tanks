namespace CryGameCode.Tanks
{
	public class Laser : TankTurret
	{
		public Laser(Tank tank) : base(tank) { }
		private Laser() { }

		public override string Model { get { return "objects/tanks/turret_laser.chr"; } }
	}
}
