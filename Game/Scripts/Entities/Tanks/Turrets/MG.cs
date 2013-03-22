using System;
using CryEngine;
using CryGameCode.Projectiles;

namespace CryGameCode.Tanks
{
	public class MG : TankTurret
	{

		static float impulseStrength = 5;

		public MG(Tank tank) : base(tank) { }

		protected override void OnFire(Vec3 firePos)
		{
			var muzzleFlash = ParticleEffect.Get("weapon_fx.tank.tank125.muzzle_flash.muzzle_flash_small");
			muzzleFlash.Spawn(firePos, TurretEntity.Rotation.Column1, 0.35f);

			Owner.Physics.AddImpulse(-TurretEntity.Rotation.Column1 * impulseStrength);
		}

		public override string Model { get { return "objects/tanks/turret_mg.chr"; } }
		public override string LeftHelper { get { return "turret_term_2"; } }
		public override string RightHelper { get { return "turret_term_1"; } }
		public override Type ProjectileType { get { return typeof(MGBullet); } }
		public override float TimeBetweenShots { get { return 0.1f; } }
		public override bool AutomaticFire { get { return true; } }
	}
}