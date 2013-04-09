using CryEngine;

using CryGameCode.Tanks;
using CryGameCode.Extensions;

namespace CryGameCode.Entities.Collectibles
{
	public class HealthCollectible : Collectible
	{
		public override void OnCollected(Tank tank)
		{
			var singlePlayer = GameRules.Current as SinglePlayer;

			var modifierManager = singlePlayer.GetExtension<GameModifierManagerExtension>();
			var modifier = modifierManager.Add<HealthModifier>(tank.Id, HealthRestoration, RestorationTime);
		}

		public override string Model
		{
			get { return "objects/tank_gameplay_assets/pickup_hologram/health_pickup.cga"; }
		}

		public override string TypeName
		{
			get { return "Regeneration"; }
		}

		public static float HealthRestoration = 35;

		/// <summary>
		/// Time in seconds to restore health, aborted it entity is attacked.
		/// </summary>
		public static float RestorationTime = 2;
	}
}
