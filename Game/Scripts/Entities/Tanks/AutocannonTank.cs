using CryEngine;

namespace CryGameCode.Tanks
{
	[Entity(Category = "Tanks")]
	public class AutocannonTank : Tank
	{
		public override string TurretModel { get { return "objects/tanks/turret_autocannon.chr"; } }
	}
}
