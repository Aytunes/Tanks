using System;
using CryEngine;
using CryGameCode.Projectiles;

namespace CryGameCode.Tanks
{
	public class Heavy : TankTurret
	{
		#region Statics
		static Heavy()
		{
			CVar.RegisterFloat("g_tankFireRecoilStrength", ref impulseStrength);
		}

		static float impulseStrength = 5;
		#endregion

		public Heavy(Tank tank) : base(tank) { }
        private Heavy() { }

		protected override void OnFire(Vec3 firePos)
		{
			var muzzleFlash = ParticleEffect.Get("weapon_fx.tank.tank125.muzzle_flash.muzzle_flash");
			muzzleFlash.Spawn(firePos, Attachment.Rotation.Column1, 0.5f);

			Owner.Physics.AddImpulse(-Attachment.Rotation.Column1 * impulseStrength);
		}

		public override string Model { get { return "objects/tanks/turret_heavy.chr"; } }
		public override Type ProjectileType { get { return typeof(HeavyShell); } }
	}
}
