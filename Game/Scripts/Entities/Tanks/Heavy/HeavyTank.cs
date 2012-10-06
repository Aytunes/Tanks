using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CryEngine;

using CryGameCode.Projectiles;

namespace CryGameCode.Tanks
{
	public class HeavyTank : Tank
	{
		public override string TurretModel { get { return "objects/tanks/turret_heavy.chr"; } }

		public override void FireWeapon(Vec3 mouseWorldPos)
		{
			var rocket = Entity.Spawn<Rocket>("1337rocket", Turret.Position, Turret.Rotation);
		}
	}
}
