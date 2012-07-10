using CryEngine;

namespace CryGameCode.Entities.Physics
{
	[Entity(Category = "Samples")]
	public class Dragon : Entity
	{
		public override void OnSpawn()
		{
			LoadObject("Objects/characters/Dragon/Dragon.cdf");
		}

		protected override void OnReset(bool enteringGame)
		{
			PlayAnimation("2landing");
		}
	}
}
