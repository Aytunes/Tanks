using System;
using CryEngine;
using CryGameCode.Projectiles;


namespace CryGameCode.Tanks
{
	public class Chaingun : TankTurret
	{
		public Chaingun(Tank tank) : base(tank) { }
        private Chaingun() { }

        protected override void OnFire(Vec3 firePos)
        {
            var muzzleFlash = ParticleEffect.Get("weapon_fx.tank.tank125.muzzle_flash.muzzle_flash_small");
            muzzleFlash.Spawn(firePos, Entity.Rotation.Column1, 0.35f);
        }	

		public override string Model { get { return "objects/tanks/turret_chaingun.chr"; } }
        public override Type ProjectileType { get { return typeof(ChaingunBullet); } }		
		public override bool AutomaticFire { get { return true; } }
		public override float TimeBetweenShots { get { return 0.05f; } }
	}
}
