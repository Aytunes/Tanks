using CryEngine;
using CryGameCode.Entities;

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

            var physicalizationParams = new PhysicalizationParams(PhysicalizationType.Particle);

            physicalizationParams.mass = Mass;
            physicalizationParams.slot = 0;

            float radius = 0.005f;
            physicalizationParams.particleParameters.thickness = radius * 2;
            physicalizationParams.particleParameters.size = radius * 2;

            physicalizationParams.particleParameters.kAirResistance = 0;
            physicalizationParams.particleParameters.kWaterResistance = 0.5f;
            physicalizationParams.particleParameters.gravity = new Vec3(0, 0, -9.81f);
            physicalizationParams.particleParameters.accThrust = 0;
            physicalizationParams.particleParameters.accLift = 0;

            physicalizationParams.particleParameters.iPierceability = 8;

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

            // TODO: Set projectile surface type.
            //physicalizationParams.particleParameters.surface_idx = 0;


            Physicalize(physicalizationParams);

			ViewDistanceRatio = 255;
		}

		public void Launch()
		{
			if (!Game.IsServer)
				return;

			RemoteLaunch(Position, Rotation);
			RemoteInvocation(RemoteLaunch, NetworkTarget.ToAllClients, Position, Rotation);
		}

		private Vec3 m_firePos;

		[RemoteInvocation]
		private void RemoteLaunch(Vec3 pos, Quat rot)
		{
			Fired = true;

			if (Game.IsPureClient)
				Hidden = false;

			Position = pos;
			m_firePos = pos;

			Rotation = rot;
			var dir = rot.Column1;
			Physics.AddImpulse(dir * Speed);

			if (DebugEnabled)
				Debug.DrawDirection(Position, 1, dir * Speed, Color.White, 1);
		}

		[RemoteInvocation]
		private void RemoteHit(Vec3 pos, Vec3 original)
		{
			Debug.DrawLine(original, pos, Color.White, 1);
			Debug.DrawSphere(pos, 1, Color.White, 1f);
		}

		protected override void OnCollision(EntityId targetEntityId, Vec3 hitPos, Vec3 dir, short materialId, Vec3 contactNormal)
		{
			// In standby waiting to be fired, don't track collisions.
			if (!Fired)
				return;

			if (Game.IsServer && DebugEnabled)
				RemoteInvocation(RemoteHit, NetworkTarget.ToAllClients, hitPos, m_firePos);

			// Id 0 is the terrain
			if (targetEntityId != 0)
			{
				var target = Entity.Get(targetEntityId);
                if (target != null)
                {
                    var damageableTarget = target as IDamageable;
                    if (damageableTarget != null)
                        damageableTarget.Damage(Damage, DamageType, hitPos, dir);
                }

                if (TargetModifier != null)
                {
                    TargetModifier.Target = target;

                    var singlePlayer = GameRules.Current as SinglePlayer;
                    singlePlayer.AddGameModifier(TargetModifier);

                    TargetModifier.Begin();
                }
			}

			if (ShouldExplode)
			{
				var explosion = new Explosion
				{
					Epicenter = Position,
					EpicenterImpulse = Position,
					Direction = dir,
					MinRadius = MinimumExplosionRadius,
					Radius = ExplosionRadius,
					MaxRadius = MaximumExplosionRadius,
					ImpulsePressure = 0.0f//ExplosionPressure Disabled for now causes movement bugs
				};

				explosion.Explode();
			}

			if (Game.IsPureClient)
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

        public IGameModifier TargetModifier { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this projectile was fired.
		/// If false, this projectile is ready for re-use.
		/// </summary>
		public bool Fired { get; set; }
	}
}
