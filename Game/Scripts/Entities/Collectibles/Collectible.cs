using CryEngine;
using CryGameCode.Projectiles;
using CryGameCode.Tanks;

namespace CryGameCode.Entities.Collectibles
{
	public abstract class Collectible : Entity
	{
		public override void OnSpawn()
		{
			Reset();

			ReceiveUpdates = true;
		}

		void Reset()
		{
			LoadObject(Model);
			HologramMaterial = Material.GetSubmaterial(1);

			PlayAnimation("Default", AnimationFlags.Loop);

			Physics.Type = PhysicalizationType.Rigid;
			Physics.Mass = -1;

			if(Minimum == null)
				Minimum = Vec3.Zero;
			if(Maximum == null)
				Maximum = Vec3.Zero;

			TriggerBounds = new BoundingBox(Minimum, Maximum);
			Active = true;
		}

		public override void OnUpdate()
		{
			if(!Active && Time.FrameStartTime - LastUsage > (DelayBetweenUsages * 1000))
			{
				Active = true;

				Material = Material.Find("objects/tank_gameplay_assets/pickup_hologram/pickups");
			}
		}

		protected override void OnEnterArea(EntityId entityId, int areaEntityId, float fade)
		{
			// Pick up if active

			if(Active)
			{
				var entity = Entity.Get(entityId);
				if(entity is Attachment)
					return;

				if(entity is Tank)
				{
					LastUser = entity as Tank;
					Collect();

					//Debug.DrawText("nom nom nom", 3.0f, Color.Blue, 5.0f);
				}
				else if(entity is Projectile)
					Debug.DrawText("DENIED", 3.0f, Color.Red, 5.0f);

				LastUsage = Time.FrameStartTime;

				Material = Material.Find("objects/tank_gameplay_assets/pickup_hologram/pickups_off");
				Active = false;
			}
		}

		public abstract void Collect();

		public float LastUsage { get; set; }
		public Tank LastUser { get; set; }

		public bool Active { get; set; }

		public float DelayBetweenUsages = 5;

		Vec3 min;
		[EditorProperty]
		public Vec3 Minimum { get { return min; } set { min = value; Reset(); } }

		Vec3 max;
		[EditorProperty]
		public Vec3 Maximum { get { return max; } set { max = value; Reset(); } }

		public Material HologramMaterial { get; set; }

		public abstract string Model { get; }
	}
}
