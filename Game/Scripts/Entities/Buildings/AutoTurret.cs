using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CryEngine;

using CryGameCode.Tanks;

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
		}

		protected override void PostSerialize()
		{
			Reset();
		}

		void Reset()
		{
			ReceiveUpdates = true;

			LoadObject(Model);

			Physics.Mass = 100;
			Physics.Resting = false;
			Physics.Type = PhysicalizationType.Rigid;

			Physics.AddImpulse(new Vec3(0, 0, -1));

			Health = MaxHealth = 100;

			Hidden = false;

			Range = 500;
		}

		protected override void OnCollision(EntityId colliderId, Vec3 hitPos, Vec3 dir, short materialId, Vec3 contactNormal)
		{
			// collided with terrain
			if (colliderId == 0)
			{
				Physics.Type = PhysicalizationType.Static;
			}
		}

		public override void OnUpdate()
		{
			if (IsDead)
				return;

			var position = Position;

			var bbox = new BoundingBox(new Vec3(position.X - Range, position.Y - Range, position.Z - Range), new Vec3(position.X + Range, position.Y + Range, position.Z + Range));

			var possibleTargets = Entity.QueryProximity<Tank>(bbox);

			float closestDistanceSquared = Range * Range;
			Tank closestTank = null;

			foreach (var tank in possibleTargets)
			{
				var tankPosition = tank.Position;
				Vec3 deltaDist = tankPosition - position;

				float distanceSquared = deltaDist.LengthSquared;

				if (distanceSquared < closestDistanceSquared)
				{
					closestTank = tank;
					closestDistanceSquared = distanceSquared;
				}
			}

			if (closestTank != null)
				FireAt(closestTank);
		}

		void FireAt(EntityBase target)
		{
			if (Time.FrameStartTime > lastShot + (TimeBetweenShots * 1000))
			{
				lastShot = Time.FrameStartTime;

				var turretPos = Position + Rotation * new Vec3(2, 0, 1); // temporary offset, remove when we have helpers.
				Vec3 direction = target.Position - Position;
				direction.Normalize();

				Entity.Spawn<Projectiles.Bullet>("pain", turretPos, Quat.CreateRotationVDir(direction));
			}
		}

		public override void OnDeath(float damage, DamageType type, Vec3 pos, Vec3 dir)
		{
			Hidden = true;
		}

		public string Model { get { return "Objects/tank_gameplay_assets/droppod_turret/turretblock.cgf"; } }

		float lastShot;
		float TimeBetweenShots { get { return 0.1f; } }

		public float Range { get; set; }
	}
}
