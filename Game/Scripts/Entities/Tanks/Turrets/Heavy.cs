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

		protected override void OnFire(Vec3 firePos)
		{
		}

		public override string Model { get { return "objects/tanks/turret_heavy.chr"; } }
		public override Type ProjectileType { get { return typeof(HeavyShell); } }
	}
}
