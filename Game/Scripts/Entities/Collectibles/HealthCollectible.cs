using CryEngine;

namespace CryGameCode.Entities.Collectibles
{
	public class HealthCollectible : Collectible
	{
		public override void Collect()
		{
			//Debug.LogAlways("Tank {0} collected health", LastUser.Name);
			remainingHeal = HealthRestoration;

			/*LastUser.OnDamaged += (damage, type) => 
			{
				LastUser = null;
			};*/
		}

		public override void OnUpdate()
		{
			base.OnUpdate();

			if (LastUser != null && !LastUser.IsDestroyed)
			{
				var heal = HealthRestoration * Time.DeltaTime * (1 / RestorationTime);
				remainingHeal -= heal;

				//LastUser.Heal(heal);

				if (remainingHeal <= 0)
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

		float remainingHeal;
	}
}
