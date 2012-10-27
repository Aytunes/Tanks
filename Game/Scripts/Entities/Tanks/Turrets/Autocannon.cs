using CryEngine;

namespace CryGameCode.Tanks
{
	public class Autocannon : TankTurret
	{
		public Autocannon(Tank tank) : base(tank) { }

		public override string Model { get { return "objects/tanks/turret_autocannon.chr"; } }
	}
}
