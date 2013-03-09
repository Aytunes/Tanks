using CryEngine;

using CryGameCode.Tanks;

namespace CryGameCode.Entities.Collectibles
{
	public class HasteCollectible : Collectible
	{
		public override void OnCollected(Tank tank)
		{
		}

		public override string Model
		{
			get { return "objects/tank_gameplay_assets/pickup_hologram/powerup_pickup.cga"; }
		}

		public override string TypeName
		{
			get { return "Haste"; }
		}
	}
}
