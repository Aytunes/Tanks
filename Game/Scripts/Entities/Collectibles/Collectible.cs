using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CryEngine;
using CryGameCode.Tanks;
using CryGameCode.Projectiles;

namespace CryGameCode.Entities.Collectibles
{
	//[Entity(Flags = EntityClassFlags.Invisible)]
	public class Collectible : Entity
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

			if(Minimum == null)
				Minimum = Vec3.Zero;
			if(Maximum == null)
				Maximum = Vec3.Zero;

			TriggerBounds = new BoundingBox(Minimum, Maximum);
			Active = true;
		}

		public override void OnUpdate()
		{
			if (!Active && Time.FrameStartTime - LastUsage > (DelayBetweenUsages * 1000))
			{
				Active = true;
				HologramMaterial.Opacity = 100;
			}
		}

		protected override void OnEnterArea(EntityId entityId, int areaEntityId, float fade)
		{
			// Pick up if active
			
			if(Active)
			{
				var entity = Entity.Get(entityId);
				if (entity is Tank)
				{
					Debug.DrawText("nom nom nom", 3.0f, Color.Blue, 5.0f);

					LastUsage = Time.FrameStartTime;

					HologramMaterial.Opacity = 0;
					Active = false;
				}
				else if (entity is Rocket) // TODO: Change to generic projectile class
				{
					Debug.DrawText("DENIED", 3.0f, Color.Red, 5.0f);

					LastUsage = Time.FrameStartTime;

					HologramMaterial.Opacity = 0;
					Active = false;
				}
			}
		}

		public float LastUsage { get; set; }
		public bool Active { get; set; }

		public float DelayBetweenUsages = 5;

		Vec3 min;
		[EditorProperty]
		public Vec3 Minimum { get { return min; } set { min = value; Reset(); } }

		Vec3 max;
		[EditorProperty]
		public Vec3 Maximum { get { return max; } set { max = value; Reset(); } }

		public Material HologramMaterial { get; set; }

		public string Model { get { return "objects/tank_gameplay_assets/pickup_hologram/powerup_pickup.cga"; } }
	}
}
