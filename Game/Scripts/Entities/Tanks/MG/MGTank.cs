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

		public override void FireWeapon(Vec3 mouseWorldPos)
		{
			var jointAbsolute = Turret.GetJointAbsolute("turret_term");
			jointAbsolute.T = Turret.Transform.TransformPoint(jointAbsolute.T);

			var bullet = Entity.Spawn<Bullet>("awesomeBullet", jointAbsolute.T, Turret.Rotation);
		}
		
		public override float TankSpeed
		{
			get
			{
				return 10;
			}
		}		
	}
}
