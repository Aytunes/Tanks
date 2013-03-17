using CryEngine;

using CryGameCode.Tanks;

namespace CryGameCode.Entities.Collectibles
{
	public class TimeScaleCollectible : Collectible
	{
		public override void OnCollected(Tank tank)
		{
			var singlePlayer = GameRules.Current as SinglePlayer;

			var modifier = new TimeScaleModifier(TimeScale, Duration);
			modifier.Begin();

			singlePlayer.AddGameModifier(modifier);

			modifier.OnEnd += () => { Remove(); };
		}

		public override string Model
		{
			get { return "objects/tank_gameplay_assets/pickup_hologram/powerup_pickup.cga"; }
		}

		public override string TypeName
		{
			get { return "Bullet time"; }
		}

		public static float TimeScale = 0.1f;

		public static float Duration = 5.0f;
	}
}
