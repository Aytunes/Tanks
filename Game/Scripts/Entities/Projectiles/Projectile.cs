using CryEngine;
using CryGameCode.Entities;

namespace CryGameCode.Projectiles
{
	public abstract class Projectile : Entity
	{
		public override void OnSpawn()
		{
			LoadObject(Model);

			Physics.Type = PhysicalizationType.Rigid;
			Physics.Mass = Mass;
			Physics.Slot = 0;

			Launch();
		}

		public virtual void Launch()
		{
			var dir = Rotation.Column1;
			Physics.AddImpulse(dir * Speed);
		}

		protected override void OnCollision(EntityId targetEntityId, Vec3 hitPos, Vec3 dir, short materialId, Vec3 contactNormal)
		{
			var effect = ParticleEffect.Get(Effect);
			effect.Spawn(hitPos, contactNormal, EffectScale);

			// Id 0 is the terrain
			if(targetEntityId != 0)
			{
				var target = Entity.Get(targetEntityId) as IDamageable;

				if(target != null)
					target.Damage(Damage, DamageType);
			}

			if(ShouldExplode)
			{
				var explosion = new Explosion
				{
					Epicenter = Position,
					EpicenterImpulse = Position,
					Direction = dir,
                    MinRadius = MinimumExplosionRadius,
                    Radius = ExplosionRadius,
                    MaxRadius = MaximumExplosionRadius,
                    ImpulsePressure = ExplosionPressure
				};

				explosion.Explode();
			}

			Remove();
		}

		public abstract string Model { get; }
		public abstract float Mass { get; }
		public abstract float Speed { get; }
		public abstract float Damage { get; }
		public abstract string Effect { get; }
		public abstract float EffectScale { get; }
		public abstract DamageType DamageType { get; }

		public virtual bool ShouldExplode { get { return false; } }
        public virtual float MinimumExplosionRadius { get { return 10; } }
        public virtual float ExplosionRadius { get { return 15; } }
        public virtual float MaximumExplosionRadius { get { return 30; } }
        public virtual float ExplosionPressure { get { return 200; } }
	}
}
