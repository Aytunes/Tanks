using CryEngine;
using CryGameCode.Entities;
using CryGameCode.Network;

namespace CryGameCode.Projectiles
{
	public abstract class Projectile : Entity
	{
		static int debugProjectiles = 0;
		static int recyleProjectiles = 1;

		static Projectile()
		{
			CVar.RegisterInt("tank_debugProjectiles", ref debugProjectiles, flags: CVarFlags.ReadOnly);
			CVar.RegisterInt("tank_recycleProjectiles", ref recyleProjectiles, flags: CVarFlags.ReadOnly);
		}

		public static bool DebugEnabled { get { return debugProjectiles != 0; } }
		public static bool RecyclingEnabled { get { return recyleProjectiles != 0; } }

		public override void OnSpawn()
		{
			LoadObject(Model);

			GameObject.SetAspectProfile(EntityAspects.Physics, (ushort)PhysicalizationType.Particle);
		}

		public void Launch(EntityId shooterId)
		{
			NetworkValidator.Server("Projectile launching");

			RemoteLaunch(shooterId, Position, Rotation, Speed);
			RemoteInvocation(RemoteLaunch, NetworkTarget.ToRemoteClients, shooterId, Position, Rotation, Speed);
		}

		private Vec3 m_firePos;

		[RemoteInvocation]
		private void RemoteLaunch(EntityId shooterId, Vec3 pos, Quat rot, float speed)
		{
			ShooterId = shooterId;
			Fired = true;

			if (Game.IsPureClient)
				Hidden = false;

			Position = pos;
			m_firePos = pos;

			Rotation = rot;
			var dir = rot.Column1;

			var physicalizationParams = new PhysicalizationParams(PhysicalizationType.Particle);

			physicalizationParams.mass = Mass;
			physicalizationParams.slot = 0;

			float radius = 0.005f;
			physicalizationParams.particleParameters.thickness = radius * 2;
			physicalizationParams.particleParameters.size = radius * 2;

			physicalizationParams.particleParameters.kAirResistance = 0.5f;
			physicalizationParams.particleParameters.kWaterResistance = 0.5f;
			physicalizationParams.particleParameters.gravity = new Vec3(0, 0, -9.81f);
			physicalizationParams.particleParameters.accThrust = 0;
			physicalizationParams.particleParameters.accLift = 0;

			physicalizationParams.particleParameters.iPierceability = 8;
			physicalizationParams.particleParameters.surface_idx = Material.SurfaceType.Id;

			physicalizationParams.particleParameters.velocity = speed;
			physicalizationParams.particleParameters.heading = dir;

			physicalizationParams.particleParameters.flags |= PhysicalizationFlags.LogCollisions;
			physicalizationParams.particleParameters.flags |= PhysicalizationFlags.MonitorCollisions;

			var singleContact = true;
			if (singleContact)
				physicalizationParams.particleParameters.flags |= PhysicalizationFlags.Particle_SingleContact;

			var noRoll = false;
			if (noRoll)
				physicalizationParams.particleParameters.flags |= PhysicalizationFlags.Particle_NoRoll;

			var noSpin = false;
			if (noSpin)
				physicalizationParams.particleParameters.flags |= PhysicalizationFlags.Particle_NoSpin;

			var noPathAlignment = false;
			if (noPathAlignment)
				physicalizationParams.particleParameters.flags |= PhysicalizationFlags.Particle_NoPathAlignment;

			Physicalize(physicalizationParams);

			ViewDistanceRatio = 255;

			if (DebugEnabled)
				Debug.DrawDirection(Position, 1, dir * Speed, Color.White, 1);
		}

		[RemoteInvocation]
		private void RemoteHit(Vec3 pos, Vec3 original)
		{
			Debug.DrawLine(original, pos, Color.White, 1);
			Debug.DrawSphere(pos, 1, Color.White, 1f);
		}

		protected override void OnCollision(ColliderInfo source, ColliderInfo target, Vec3 hitPos, Vec3 contactNormal, float penetration, float radius)
		{
			// In standby waiting to be fired, don't track collisions.
			if (!Fired)
				return;

			if (Game.IsServer && DebugEnabled)
				RemoteInvocation(RemoteHit, NetworkTarget.ToAllClients, hitPos, m_firePos);

			var otherEntity = source.Entity;
			if (otherEntity == this)
				otherEntity = target.Entity;

			if (Game.IsServer && otherEntity != null)
			{
				var totalDamage = (source.velocity.Length / Speed) * Damage;

				var damageableTarget = otherEntity as IDamageable;
				if (damageableTarget != null)
					damageableTarget.Damage(ShooterId, totalDamage, DamageType, hitPos, Vec3.Zero);

				if (TargetModifier != null)
				{
					TargetModifier.Target = otherEntity;

					var singlePlayer = GameRules.Current as SinglePlayer;
					singlePlayer.AddGameModifier(TargetModifier);

					TargetModifier.Begin();
				}
			}

			if (Game.IsClient)
			{
				var effect = ParticleEffect.Get(Effect);
				if (effect != null)
					effect.Spawn(hitPos, contactNormal, EffectScale);
			}

			if (ShouldExplode)
			{
				var explosion = new Explosion
				{
					Epicenter = Position,
					EpicenterImpulse = Position,
					Direction = Vec3.Zero,
					MinRadius = MinimumExplosionRadius,
					Radius = ExplosionRadius,
					MaxRadius = MaximumExplosionRadius,
					ImpulsePressure = 0.0f//ExplosionPressure Disabled for now causes movement bugs
				};

				explosion.Explode();

				/*if (Game.IsServer)
				{
					foreach (var affectedPhysicalEntity in explosion.AffectedEntities)
					{
						var entity = affectedPhysicalEntity.Owner;
						var damageable = entity as IDamageable;
						if (damageable == null)
							continue;

						var distance = System.Math.Abs((Position - entity.Position).Length);

						var damage = ExplosionRelativeDamage * (1 - (distance / MaximumExplosionRadius));

						damageable.Damage(0, damage, DamageType.Explosive, Vec3.Zero, Vec3.Zero);
					}
				}*/
			}

			if (Game.IsPureClient || Game.IsEditor)
				Hidden = true;
            
			Fired = false;
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
        public virtual float ExplosionRelativeDamage { get { return 5; } }

		public IGameModifier TargetModifier { get; set; }

		public EntityId ShooterId { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this projectile was fired.
		/// If false, this projectile is ready for re-use.
		/// </summary>
		public bool Fired { get; set; }
	}
}
