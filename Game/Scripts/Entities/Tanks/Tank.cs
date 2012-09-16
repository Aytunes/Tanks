using CryEngine;

namespace CryGameCode.Tanks
{
	public class Tank : Entity
	{
		static Tank()
		{
			CVar.RegisterFloat("g_tankTurretMinAngle", ref tankTurretMinAngle);
			CVar.RegisterFloat("g_tankTurretMaxAngle", ref tankTurretMaxAngle);

			CVar.RegisterFloat("g_tankTurretTurnSpeed", ref tankTurretTurnSpeed);
		}

		public override void OnSpawn()
		{
			Input.MouseEvents += ProcessMouseEvents;

			// TODO: Allow picking tank
			LoadObject("objects/tanks/tank_laser.cdf");

			Physics.Type = PhysicalizationType.Rigid;
			Physics.Mass = 500;
			Physics.Resting = false;

			Input.RegisterAction("moveright", OnMoveRight);
			Input.RegisterAction("moveleft", OnMoveLeft);
			Input.RegisterAction("moveforward", OnMoveForward);
			Input.RegisterAction("moveback", OnMoveBack);

			ReceiveUpdates = true;
		}

		protected override bool OnRemove()
		{
			Input.UnregisterActions(this);

			Input.MouseEvents -= ProcessMouseEvents;

			return true;
		}

		public override void OnUpdate()
		{
			Vec3 mousePos = Renderer.ScreenToWorld((int)Input.MousePosition.X, (int)Input.MousePosition.Y);

			Debug.DrawSphere(mousePos, 0.3f, Color.Red, 2);

			Vec3 dir = mousePos - Position;

			float yaw = Math.RadiansToDegrees(Math.Atan2(dir.Y, dir.X)) - 90;
			yaw = Math.ClampAngle(yaw, tankTurretMinAngle, tankTurretMaxAngle);
			yaw = Math.DegreesToRadians(yaw);

			var rot = Rotation;

			rot.SetRotationZ(yaw);
			Rotation = rot;
		}

		private void ProcessMouseEvents(MouseEventArgs e)
		{
			switch (e.MouseEvent)
			{
				// Handle turret rotation
				case MouseEvent.Move:
					{
						var mousePos = Renderer.ScreenToWorld(e.X, e.Y);

						
					}
					break;
			}
		}

		private void OnMoveRight(ActionMapEventArgs e)
		{
			Debug.LogAlways("Moving right");

			Position += Rotation.Column0 * 1;
		}

		private void OnMoveLeft(ActionMapEventArgs e)
		{
			Debug.LogAlways("Moving left");

			//Position += Rotation.Column0 * -1;

			var moveRequest = new EntityMovementRequest();
			moveRequest.type = EntityMoveType.Normal;
			moveRequest.velocity = Rotation.Column0 * -10;

			AddMovement(ref moveRequest);
		}

		private void OnMoveForward(ActionMapEventArgs e)
		{
			Debug.LogAlways("Moving forward");

			Position += Rotation.Column1 * 1;
		}

		private void OnMoveBack(ActionMapEventArgs e)
		{
			Debug.LogAlways("Moving back");

			Position += Rotation.Column1 * -1;
		}

		Vec3 Angles;

		static float tankTurretMinAngle = -20;
		static float tankTurretMaxAngle = 80;

		static float tankTurretTurnSpeed = 250;
	}
}
