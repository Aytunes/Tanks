using System;
using CryEngine;

namespace CryGameCode.Tanks
{
	public class Rocket : TankTurret
	{
		const float impulseStrength = 400;

		public Rocket(Tank tank) : base(tank) { }

		protected override void OnFire(Vec3 firePos)
		{
			if (Game.IsClient)
			{
				var muzzleFlash = ParticleEffect.Get("weapon_fx.tank.tank125.muzzle_flash.muzzle_flash");
				muzzleFlash.Spawn(firePos, TurretEntity.Rotation.Column1, 0.5f);
			}

			Owner.Physics.AddImpulse(-TurretEntity.Rotation.Column1 * impulseStrength);
		}

		public override string Model { get { return "objects/tanks/turret_rocket.chr"; } }
		public override string LeftHelper { get { return "turret_term_2"; } }
		public override string RightHelper { get { return "turret_term_1"; } }
		public override float TimeBetweenShots { get { return 2f; } }
		public override Type ProjectileType { get { return typeof(Rocket); } }
	}
}
