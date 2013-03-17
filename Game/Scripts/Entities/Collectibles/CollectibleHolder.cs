using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using CryEngine;
using CryEngine.Extensions;

using CryGameCode.Projectiles;
using CryGameCode.Tanks;

namespace CryGameCode.Entities.Collectibles
{
	public class CollectibleHolder : Entity
	{
		static CollectibleHolder()
		{
			CollectibleTypes = (from type in Assembly.GetExecutingAssembly().GetTypes()
								where type.Implements<Collectible>()
								select type).ToList();
		}

		public override void OnSpawn()
		{
			Reset();

			ReceiveUpdates = true;
		}

		protected override void PostSerialize()
		{
			Activate();
		}

		void Reset()
		{
			TriggerBounds = new BoundingBox(Minimum, Maximum);

			SpawnCollectible();
			Activate();
		}

		void SpawnCollectible()
		{
			var collectibleType = CollectibleTypes[SinglePlayer.Selector.Next(CollectibleTypes.Count)];

			Collectible = Entity.Spawn("Collectible", collectibleType) as Collectible;
		}

		void Activate()
		{	
			if (Collectible != null)
			{
				// TODO: Make base model seperate, in order to change pickup model based on collectible type.
				LoadObject(Collectible.Model);

				Physics.Type = PhysicalizationType.Rigid;
				Physics.Mass = -1;

				Material = Material.Find("objects/tank_gameplay_assets/pickup_hologram/pickups");

				PlayAnimation("Default", AnimationFlags.Loop);

				Active = true;
			}
		}

		public override void OnUpdate()
		{
			if (!Active && Time.FrameStartTime - LastUsage > (DelayBetweenUsages * 1000))
				Activate();
		}

		protected override void OnEnterArea(EntityId entityId, int areaEntityId, float fade)
		{
			// Pick up if active

			if (Active)
			{
				var entity = Entity.Get(entityId);

				if (entity is Tank)
				{
					LastUser = entity as Tank;

					Debug.DrawText(string.Format("{0} GOT A {1} COLLECTIBLE!", entity.Name.ToUpper(), Collectible.TypeName.ToUpper()), 3.0f, Color.Red, 2.0f);
					Collectible.OnCollected(LastUser);

					//Debug.DrawText("nom nom nom", 3.0f, Color.Blue, 5.0f);
				}
				else if (entity is Projectile)
				{
					Debug.DrawText(string.Format("{0} DENIED A {1} COLLECTIBLE!", entity.Name.ToUpper(), Collectible.TypeName.ToUpper()), 3.0f, Color.Red, 2.0f);
				}
				else
					return;

				SpawnCollectible();

				LastUsage = Time.FrameStartTime;

				Material = Material.Find("objects/tank_gameplay_assets/pickup_hologram/pickups_off");
				Active = false;
			}
		}

		public float LastUsage { get; set; }
		public Tank LastUser { get; set; }

		public Collectible Collectible { get; set; }

		public bool Active { get; set; }

		public float DelayBetweenUsages = 5;

		Vec3 min;
		[EditorProperty]
		public Vec3 Minimum { get { return min; } set { min = value; Reset(); } }

		Vec3 max;
		[EditorProperty]
		public Vec3 Maximum { get { return max; } set { max = value; Reset(); } }

		private static List<Type> CollectibleTypes;
	}
}
