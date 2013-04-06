using CryEngine;

namespace CryGameCode.Entities.Markers
{
	[Entity(Category = "Markers")]
	public class OverviewPoint : Entity
	{
		public const int MoveLerpSpeed = 5;
		public const int RotationLerpSpeed = 5;

		public bool Active { get; set; }
	}
}
