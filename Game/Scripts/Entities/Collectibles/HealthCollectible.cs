using CryEngine;

using CryGameCode.Tanks;

namespace CryGameCode.Entities.Collectibles
{
	public class HealthCollectible : Collectible
	{
		public override void OnCollected(Tank tank)
		{
			User = tank;

			//Debug.LogAlways("Tank {0} collected health", LastUser.Name);
			remainingHeal = HealthRestoration;

			User.OnDamaged += (damage, type) =>
			{
				// cancel heal if user was damaged by someone
				User = null;
			};

			ReceiveUpdates = true;
		}

		public override void OnUpdate()
		{
			if (User != null && !User.IsDestroyed)
			{
				var heal = HealthRestoration * Time.DeltaTime * (1 / RestorationTime);
				remainingHeal -= heal;

				User.Heal(heal);

				if (remainingHeal <= 0)
					User = null;
			}
		}

		public override string Model
		{
			get { return "objects/tank_gameplay_assets/pickup_hologram/health_pickup.cga"; }
		}

		public override string TypeName
		{
			get { return "Regeneration"; }
		}

		public float HealthRestoration = 35;

		/// <summary>
		/// Time in seconds to restore health, aborted it entity is attacked.
		/// </summary>
		public float RestorationTime = 2;

		float remainingHeal;

		public Tank User { get; private set; }
	}
}
