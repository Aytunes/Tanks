using System;
using CryEngine;
using CryGameCode.Projectiles;

namespace CryGameCode.Tanks
{
	public class Autocannon : TankTurret
	{
		const float impulseStrength = 5;

		public Autocannon(Tank tank) : base(tank) { }

		protected override void OnFire(Vec3 firePos)
		{
			if (Game.IsClient)
			{
				var muzzleFlash = ParticleEffect.Get("weapon_fx.tank.tank125.muzzle_flash.muzzle_flash");
				muzzleFlash.Spawn(firePos, TurretEntity.Rotation.Column1, 0.5f);
			}

			Owner.Physics.AddImpulse(-TurretEntity.Rotation.Column1 * impulseStrength);
		}

		public override string Model { get { return "objects/tanks/turret_autocannon.chr"; } }
		public override Type ProjectileType { get { return typeof(AutocannonRocket); } }
		public override bool AutomaticFire { get { return true; } }
		public override float TimeBetweenShots { get { return 0.5f; } }
	}
}
