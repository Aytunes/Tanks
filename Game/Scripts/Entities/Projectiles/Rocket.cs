using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CryEngine;
using CryGameCode.Entities;
using CryGameCode.Tanks;

namespace CryGameCode.Projectiles
{
	[Entity(Flags=EntityClassFlags.Invisible)]
	public class Rocket : Projectile
	{
		public override void Launch()
		{
			TravelDir = Rotation.Column1;
			Physics.AddImpulse(TravelDir * Speed);
		}

		protected override void OnCollision(EntityId targetEntityId, Vec3 hitPos, Vec3 dir, short materialId, Vec3 contactNormal)
		{
			var effect = ParticleEffect.Get("explosions.explosive_bullet.default");
			effect.Spawn(hitPos);

			if(targetEntityId != 0)
			{
				var target = Entity.Get(targetEntityId) as IDamageable;

				if(target != null)
					target.OnDamage(20, DamageType.Explosive);
			}

			Remove();
		}

		public float Speed { get { return 1290; } }

		public Vec3 TravelDir { get; private set; }

		public override string Model { get { return "objects/projectiles/rocket.cgf"; } }
		public override float Mass { get { return 20; } }
	}
}
