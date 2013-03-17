using CryEngine;

using CryGameCode.Tanks;

namespace CryGameCode.Entities.Collectibles
{
	/*public class InvisibilityCollectible : Collectible
	{
		public override void OnCollected(Tank tank)
		{
			var singlePlayer = GameRules.Current as SinglePlayer;

			var modifier = new InvisibilityModifier(tank, Duration);
			modifier.Begin();

			singlePlayer.AddGameModifier(modifier);

			modifier.OnEnd += () => { Remove(); };
		}

		public override string Model
		{
			get { return "objects/tank_gameplay_assets/pickup_hologram/health_pickup.cga"; }
		}

		public override string TypeName
		{
			get { return "Invisibility"; }
		}

		public static float Duration = 10;
	}*/
}
