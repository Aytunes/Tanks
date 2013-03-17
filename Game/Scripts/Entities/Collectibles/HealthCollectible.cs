﻿using CryEngine;

using CryGameCode.Tanks;

namespace CryGameCode.Entities.Collectibles
{
	public class HealthCollectible : Collectible
	{
		public override void OnCollected(Tank tank)
		{
			var singlePlayer = GameRules.Current as SinglePlayer;

			var modifier = new HealthModifier(tank, HealthRestoration, RestorationTime);
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
			get { return "Regeneration"; }
		}

		public static float HealthRestoration = 35;

		/// <summary>
		/// Time in seconds to restore health, aborted it entity is attacked.
		/// </summary>
		public static float RestorationTime = 2;
	}
}
