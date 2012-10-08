using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CryEngine;
using CryGameCode.Tanks;

namespace CryGameCode.Entities.Collectibles
{
	public class HealthCollectible : Collectible
	{
		public override void Collect()
		{
			Debug.LogAlways("Tank {0} collected health", LastUser.Name);
			RemainingRestoration = HealthRestoration;

			LastUser.OnDamaged += (damage, type) => 
			{
				RemainingRestoration = 0;
			};
		}

		public override void OnUpdate()
		{
			base.OnUpdate();

			if (LastUser != null && !LastUser.IsDestroyed)
			{
				var heal = RemainingRestoration * Time.DeltaTime;
				RemainingRestoration -= heal;

				Debug.LogAlways("Healing tank {0} with {1}HP, {2} remaining", LastUser.Name, heal, RemainingRestoration);
				LastUser.Heal(heal);

				if (RemainingRestoration <= 0)
					LastUser = null;
			}
		}

		public override string Model
		{
			 get { return "objects/tank_gameplay_assets/pickup_hologram/health_pickup.cga"; }
		}

		[EditorProperty]
		public float HealthRestoration = 35;

		/// <summary>
		/// Time in seconds to restore health, aborted it entity is attacked.
		/// </summary>
		[EditorProperty]
		public float RestorationTime = 2;

		float RemainingRestoration;
	}
}
