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
			ViewDistanceRatio = 255;
		}

		/*protected override void NetSerialize(CryEngine.Serialization.CrySerialize serialize, int aspect, byte profile, int flags)
		{
			Vec3 pos = Vec3.Zero;
			Quat rot = Quat.Identity;

			if (serialize.IsWriting)
			{
				pos = Position;
				rot = Rotation;
			}

			serialize.Value("pos", ref pos, "wrld");
			serialize.Value("rot", ref rot, "ori1");

			if (serialize.IsReading)
			{
				Position = pos;
				Rotation = rot;
			}
		}*/

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
			Hidden = false;

			Position = pos;
			m_firePos = pos;

			Rotation = rot;
			var dir = rot.Column1;
			Physics.AddImpulse(dir * Speed);
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

			if (Game.IsServer)
				RemoteInvocation(RemoteHit, NetworkTarget.ToAllClients, hitPos, m_firePos);

			var effect = ParticleEffect.Get(Effect);
			if (effect != null)
				effect.Spawn(hitPos, contactNormal, EffectScale);

			// Id 0 is the terrain
			if (targetEntityId != 0)
			{
				var target = Entity.Get(targetEntityId) as IDamageable;

				if (target != null)
					target.Damage(Damage, DamageType, hitPos, dir);
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
					ImpulsePressure = ExplosionPressure
				};

				explosion.Explode();
			}

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

		/// <summary>
		/// Gets or sets a value indicating whether this projectile was fired.
		/// If false, this projectile is ready for re-use.
		/// </summary>
		public bool Fired { get; set; }
	}
}
