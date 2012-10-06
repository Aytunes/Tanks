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
			var jointAbsolute = Turret.GetJointAbsolute("turret_term");
			jointAbsolute.T = Turret.Transform.TransformPoint(jointAbsolute.T);

			var rocket = Entity.Spawn<Rocket>("1337rocket", jointAbsolute.T, Turret.Rotation);

			var muzzleFlash = ParticleEffect.Get("weapon_fx.tank.tank125.muzzle_flash.muzzle_flash");
			muzzleFlash.Spawn(jointAbsolute.T);
		}
	}
}
