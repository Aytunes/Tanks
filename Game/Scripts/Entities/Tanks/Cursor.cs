using CryEngine;

namespace CryGameCode.Tanks
{
	[Entity(Flags = EntityClassFlags.Invisible)]
	public class Cursor : Entity
	{
		public override void OnSpawn()
		{
			LoadObject("objects/default/teapot.cgf");
			ReceiveUpdates = true;
		}

		public override void OnUpdate()
		{
			var pos = Renderer.ScreenToWorld(Input.MouseX, Input.MouseY);
			Position = new Vec3(pos.X, pos.Y, Actor.LocalClient.Position.Z);
		}
	}
}
