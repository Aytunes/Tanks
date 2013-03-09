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
				radius = 5,
				color = Color.Red,
				hdrDynamic = 2
			};

			LoadLight(light);
			LoadObject("objects/effects/particle_effects/bubble/3dbubble.cgf");

			ReceiveUpdates = true;
		}

		public override void OnUpdate()
		{
			var pos = Renderer.ScreenToWorld(Input.MouseX, Input.MouseY);

			Position = new Vec3(pos.X, pos.Y, pos.Z + 1.5f);
		}
	}
}
