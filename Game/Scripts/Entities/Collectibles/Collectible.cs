using CryEngine;
using CryGameCode.Tanks;

namespace CryGameCode.Entities.Collectibles
{
	[Entity(Flags = EntityClassFlags.Invisible)]
	public abstract class Collectible : Entity
	{
		public abstract void OnCollected(Tank tank);

		public abstract string Model { get; }
		public abstract string TypeName { get; }
	}
}
