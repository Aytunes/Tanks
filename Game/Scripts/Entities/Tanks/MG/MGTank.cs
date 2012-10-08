using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CryEngine;
using CryGameCode.Projectiles;

namespace CryGameCode.Tanks
{
	[Entity(Category = "Tanks")]
	public class MGTank : Tank
	{
		public override string TurretModel { get { return "objects/tanks/turret_mg.chr"; } }

		public override Type ProjectileType
		{
			get { throw new NotImplementedException(); }
		}
	}
}
