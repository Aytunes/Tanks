using CryEngine;

using CryGameCode.Tanks;
using CryGameCode.Extensions;

namespace CryGameCode.Entities.Collectibles
{
	public class TimeScaleCollectible : Collectible
	{
		public override void OnCollected(Tank tank)
		{
			var singlePlayer = GameRules.Current as SinglePlayer;

			var modifierManager = singlePlayer.GetExtension<GameModifierManagerExtension>();
			var modifier = modifierManager.Add<TimeScaleModifier>(TimeScale, Duration);
		}

		public override string Model
		{
			get { return "objects/tank_gameplay_assets/pickup_hologram/powerup_pickup.cga"; }
		}

		public override string TypeName
		{
			get { return "Bullet time"; }
		}

		public static float TimeScale = 0.2f;

		public static float Duration = 10.0f;
	}
}
