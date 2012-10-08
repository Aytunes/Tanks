using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CryEngine;

namespace CryGameCode.Tanks
{
	[Entity(Category = "Tanks")]
	public class RocketTank : Tank
	{
		public override string TurretModel { get { return "objects/tanks/turret_rocket.chr"; } }

		public override string LeftHelper { get { return "turret_term_2"; } }
		public override string RightHelper { get { return "turret_term_1"; } }
		public override float TimeBetweenShots { get { return 0.3f; } }
	}
}
