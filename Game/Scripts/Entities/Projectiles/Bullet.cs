using CryEngine;
using CryGameCode.Entities;

namespace CryGameCode.Projectiles
{
	[Entity(Flags = EntityClassFlags.Invisible)]
	public class Bullet : Projectile
	{
		public override float Speed { get { return 500; } }
		public override string Model { get { return "objects/projectiles/rocket.cgf"; } }
		public override float Mass { get { return 20; } }
		public override float Damage { get { return 5; } }
		public override string Effect { get { return "explosions.explosive_bullet.default"; } }
		public override DamageType DamageType { get { return DamageType.Bullet; } }
	}
}
