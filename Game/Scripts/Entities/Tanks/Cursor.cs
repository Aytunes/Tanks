using CryEngine;

namespace CryGameCode.Tanks
{
	[Entity(Flags = EntityClassFlags.Invisible)]
	public class Cursor : Entity
	{
		public override void OnSpawn()
		{
			var light = new LightParams
			{
				radius = 15,
				color = Color.Red,
				hdrDynamic = 2,
				flags = LightFlags.CastShadows
			};

			LoadLight(light);
			LoadObject("objects/effects/particle_effects/bubble/3dbubble.cgf");
			ViewDistanceRatio = 255;

			ReceiveUpdates = true;
		}

		public override void OnUpdate()
		{
			var pos = Renderer.ScreenToWorld(Input.MouseX, Input.MouseY);

			Position = new Vec3(pos.X, pos.Y, pos.Z + 1.5f);
		}
	}
}
