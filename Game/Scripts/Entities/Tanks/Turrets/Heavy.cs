using System;
using CryEngine;
using CryGameCode.Projectiles;

namespace CryGameCode.Tanks
{
	public class Heavy : TankTurret
	{
		const float impulseStrength = 200;

		public Heavy(Tank tank) : base(tank) { }

		protected override void OnFire(Vec3 firePos)
		{
			if (Game.IsClient)
			{
				var muzzleFlash = ParticleEffect.Get("weapon_fx.tank.tank125.muzzle_flash.muzzle_flash");
				muzzleFlash.Spawn(firePos, TurretEntity.Rotation.Column1, 0.5f);
			}

			Owner.Physics.AddImpulse(-TurretEntity.Rotation.Column1 * impulseStrength);
		}

		public override string Model { get { return "objects/tanks/turret_heavy.chr"; } }
		public override Type ProjectileType { get { return typeof(HeavyShell); } }

		public override string FireSound { get { return "Sounds/weapons:tank_main_cannon:fire_3p"; } }
	}
}
