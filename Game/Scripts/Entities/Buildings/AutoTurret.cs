using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CryEngine;

using CryGameCode.Tanks;
using CryGameCode.Projectiles;

namespace CryGameCode.Entities.Buildings
{
	[Entity(Category = "Buildings")]
	public class AutoTurret : DamageableEntity
	{
		protected override void OnEditorReset(bool enteringGame)
		{
			Reset();
		}

		public override void OnSpawn()
		{
			Reset();

			OnDeath += OnDied;
		}

		protected override bool OnRemove()
		{
 			 foreach (var projectile in ProjectileStorage)
				projectile.Remove();

			ProjectileStorage.Clear();

			return true;
		}

		protected override void PostSerialize()
		{
			Reset();
		}

		void Reset()
		{
			if (IsDestroyed)
				return;

			ReceiveUpdates = true;

			LoadObject(Model);

			// Physicalize
			var physicalizationParams = new PhysicalizationParams(PhysicalizationType.Rigid);

			physicalizationParams.mass = 100;

			Physicalize(physicalizationParams);

			Physics.Resting = false;
			Physics.AddImpulse(new Vec3(0, 0, -1));

			Health = MaxHealth = 200;

			Hidden = false;

			Active = false;
		}

		protected override void OnCollision(ColliderInfo source, ColliderInfo target, Vec3 hitPos, Vec3 contactNormal, float penetration, float radius)
		{
			Debug.LogAlways("Collided, target foreign id was {0} and source was {1}", target.foreignId, source.foreignId);

			// collided with terrain or a static object.
			if (!Active && (source.foreignId < PhysicsForeignIdentifiers.Entity || target.foreignId < PhysicsForeignIdentifiers.Entity))
			{
				DePhysicalize();

				Physicalize(new PhysicalizationParams(PhysicalizationType.Static));

				Active = true;

				Position = Position - new Vec3(0, 0, 1.5f);

				// pick a random direction
				var random = new Random();

				currentRotationDirection = random.Next(1) - 1;
			}
			else
			{
				var otherEntity = source.Entity;
				if(otherEntity == this)
					otherEntity = target.Entity;

				if(otherEntity != null)
					Debug.LogAlways("collided with {0}", otherEntity.Name);

				if (otherEntity is Tank && !Active) // Landed on a tank, kill it.
				{
					var tank = otherEntity as Tank;
					tank.Damage(Id, tank.Health, DamageType.Collision, hitPos, Vec3.Zero);
				}
				else if (otherEntity is Projectile) // Some heartless noob shot us. Shoot back.
				{
					var projectile = otherEntity as Projectile;

					if(projectile.ShooterId != 0)
						lastTarget = Entity.Get(projectile.ShooterId);
				}
			}
		}

		public override void OnUpdate()
		{
			if (IsDead || !Active)
				return;

			if (Time.FrameStartTime - timeSinceSeenTarget > 3000)
				lastTarget = null;

			var position = Position;

			// See if the path to the target is unobstructed.
			var hits = Ray.Cast(FireHelperPosition, Rotation.Column0.Normalized * Range, EntityQueryFlags.All, RayWorldIntersectionFlags.AnyHit);
			Debug.DrawDirection(FireHelperPosition, 3.0f, Rotation.Column0.Normalized * Range, Color.Green, 0.3f);

			if (hits.Count() > 0)
			{
				var hit = hits.ElementAt(0);

				if (hit.Entity != null)
				{
					timeSinceSeenTarget = Time.FrameStartTime;

					lastTarget = hit.Entity;
				}
			}

			if (lastTarget != null)
			{
				FireAt(lastTarget);

				// Workaround for offset auto-turret rotation pivot.
				var positionDelta = -Quat.CreateRotationVDir((lastTarget.Position - position).Normalized).Column0;

				Rotation = Quat.CreateSlerp(Rotation, Quat.CreateRotationVDir(positionDelta), Time.DeltaTime * RotationSpeedFiring);
			}
			else
			{
				// Change rotation direction randomly, but not more than once per 3s.
				if (Time.FrameStartTime - timeSinceLastRotationChange > 3000)
				{
					var random = new Random();

					if (random.Next(100) == 0) // change rotation direction
					{
						currentRotationDirection *= -1;

						timeSinceLastRotationChange = Time.FrameStartTime;
					}
				}

				Rotation *= Quat.CreateRotationZ(currentRotationDirection * Time.DeltaTime * RotationSpeed);
			}
		}

		public Vec3 FireHelperPosition
		{
			get
			{
				return Position + Rotation * new Vec3(2.5f, 0, 2.5f);
			}
		}

		void FireAt(EntityBase target)
		{
			if (Time.FrameStartTime > lastShot + (TimeBetweenShots * 1000))
			{
				Debug.LogAlways("Firing at {0}", target.Name);

				lastShot = Time.FrameStartTime;

				var projectile = ProjectileStorage.FirstOrDefault(x => !x.Fired);
				if (projectile != null && projectile.IsDestroyed)
				{
					ProjectileStorage.Remove(projectile);
					projectile = null;
				}

				if (projectile == null || !Projectile.RecyclingEnabled)
				{
					projectile = CryEngine.Entity.Spawn<Bullet>("pain", FireHelperPosition, Quat.CreateRotationVDir(Rotation.Column0)) as Projectile;
					ProjectileStorage.Add(projectile);
				}
				else	
				{
					projectile.Position = FireHelperPosition;
					projectile.Rotation = Quat.CreateRotationVDir(Rotation.Column0);
				}

				projectile.Launch(this.Id);
			}
		}

		public void OnDied(EntityId sender, float damage, DamageType type, Vec3 pos, Vec3 dir)
		{
			Active = false;
			Hidden = true;
		}

		public string Model { get { return "Objects/tank_gameplay_assets/droppod_turret/turretblock.cgf"; } }

		float lastShot;
		float TimeBetweenShots { get { return 0.1f; } }

		public float timeSinceSeenTarget;
		EntityBase lastTarget;

		int currentRotationDirection = 1;
		float timeSinceLastRotationChange;

		public float RotationSpeed { get { return 2.0f; } }
		/// <summary>
		/// Rotation speed when a target has been found
		/// </summary>
		public float RotationSpeedFiring { get { return 2.0f; } }

		public float Range { get { return 35; } }

		public bool Active { get; set; }

		HashSet<Projectile> ProjectileStorage = new HashSet<Projectile>();
	}
}
