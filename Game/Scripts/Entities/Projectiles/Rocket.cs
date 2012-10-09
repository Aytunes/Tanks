using CryEngine;
using CryGameCode.Entities;

namespace CryGameCode.Projectiles
{
	[Entity(Flags = EntityClassFlags.Invisible)]
	public class Rocket : Projectile
	{
		public override string Model { get { return "objects/projectiles/tank_rocket.cgf"; } }
		public override float Mass { get { return 20; } }
		public override float Speed { get { return 1290; } }
		public override string Effect { get { return "explosions.C4_explosion.c4"; } }
		public override float EffectScale { get { return 0.5f; } }
		public override float Damage { get { return 20; } }
		public override DamageType DamageType { get { return DamageType.Explosive; } }
	}
}
