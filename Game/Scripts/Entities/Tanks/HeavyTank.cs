using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CryEngine;

using CryGameCode.Projectiles;

namespace CryGameCode.Tanks
{
	[Entity(Category="Tanks")]
	public class HeavyTank : Tank
	{
		static HeavyTank()
		{
			CVar.RegisterFloat("g_tankFireRecoilStrength", ref impulseStrength);
		}

		static float impulseStrength = 5;

		public override string TurretModel { get { return "objects/tanks/turret_heavy.chr"; } }
		public override Type ProjectileType { get { return typeof(Rocket); } }

		protected override void OnFire(Vec3 firePos)
		{
			var muzzleFlash = ParticleEffect.Get("weapon_fx.tank.tank125.muzzle_flash.muzzle_flash");
			muzzleFlash.Spawn(firePos, Turret.Rotation.Column1, 0.5f);

			Physics.AddImpulse(-Turret.Rotation.Column1 * impulseStrength);
		}
	}
}
