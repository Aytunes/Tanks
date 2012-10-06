using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryGameCode.Tanks
{
	public class RocketTank : Tank
	{
		public override string TurretModel { get { return "objects/tanks/turret_rocket.chr"; } }
	}
}
