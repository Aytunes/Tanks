using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CryEngine;

namespace CryGameCode.Tanks
{
	[Entity(Category = "Tanks")]
	public class MGTank : Tank
	{
		public override string TurretModel { get { return "objects/tanks/turret_mg.chr"; } }

		public override void FireWeapon(Vec3 mouseWorldPos)
		{
			throw new NotImplementedException();
		}
	}
}
