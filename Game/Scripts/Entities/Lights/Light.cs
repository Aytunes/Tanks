using CryEngine;

namespace CryGameCode.Entities.Lights
{
	[Entity(Category = "Lights")]
	public class Light : Entity
	{
		public override void OnSpawn()
		{
			LightSource = new LightSource(Position);

			LightSource.Color = Color.Red;
			LightSource.HDRDynamic = 10;
			LightSource.Radius = 10.0f;
		}

		protected override void OnMove()
		{
			LightSource.Origin = Transform.GetTranslation();
			LightSource.Transform = Transform;
		}

		LightSource LightSource { get; set; }
	}
}