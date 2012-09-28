using CryEngine;

namespace CryGameCode.Tanks
{
	public abstract class Tank : Entity
	{
		static Tank()
		{
			CVar.RegisterFloat("g_tankTurretMinAngle", ref tankTurretMinAngle);
			CVar.RegisterFloat("g_tankTurretMaxAngle", ref tankTurretMaxAngle);

			CVar.RegisterFloat("g_tankTurretTurnSpeed", ref tankTurretTurnSpeed);
		}

		public string Model { get { return "objects/tanks/tank_generic.cdf"; } }
		public abstract string TurretModel { get; }

		public override void OnSpawn()
		{
			LoadObject(Model);

			Turret = GetAttachment("turret");
			Turret.LoadObject(TurretModel);

			Physics.Type = PhysicalizationType.Living;
			Physics.Mass = 500;
			Physics.Resting = false;

			Input.RegisterAction("moveright", OnMoveRight);
			Input.RegisterAction("moveleft", OnMoveLeft);
			Input.RegisterAction("moveforward", OnMoveForward);
			Input.RegisterAction("moveback", OnMoveBack);

			Input.RegisterAction("attack1", OnShoot);

			Input.MouseEvents += ProcessMouseEvents;

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
			
		}

		protected override void OnPrePhysicsUpdate()
		{
			var moveRequest = new EntityMovementRequest();
			moveRequest.type = EntityMoveType.Normal;
			moveRequest.velocity = VelocityRequest;

			//Velocity = VelocityRequest;
			moveRequest.rotation = Quat.Identity;
			AddMovement(ref moveRequest);
		}

		private void ProcessMouseEvents(MouseEventArgs e)
		{
			switch (e.MouseEvent)
			{
				// Handle turret rotation
				case MouseEvent.Move:
					{
						var mousePos = Renderer.ScreenToWorld(e.X, e.Y);

						Debug.DrawSphere(mousePos, 0.3f, Color.Red, 2);

						Vec3 dir = mousePos - Turret.Position;

						float yaw = Math.RadiansToDegrees(Math.Atan2(dir.Y, dir.X));
						yaw = Math.ClampAngle(yaw, tankTurretMinAngle, tankTurretMaxAngle);
						yaw = Math.DegreesToRadians(yaw);

						var rot = Turret.Rotation;

						rot.SetRotationZ(yaw);
						Turret.Rotation = rot;
					}
					break;
			}
		}

		private void OnMoveRight(ActionMapEventArgs e)
		{
			Debug.LogAlways("Moving right");

			VelocityRequest += Rotation.Column0 * 10;
		}

		private void OnMoveLeft(ActionMapEventArgs e)
		{
			Debug.LogAlways("Moving left");

			VelocityRequest = Rotation.Column0 * -10;
		}

		private void OnMoveForward(ActionMapEventArgs e)
		{
			Debug.LogAlways("Moving forward");

			VelocityRequest += Rotation.Column1 * 10;
		}

		private void OnMoveBack(ActionMapEventArgs e)
		{
			Debug.LogAlways("Moving back");

			VelocityRequest += Rotation.Column1 * -10;
		}

		private void OnShoot(ActionMapEventArgs e)
		{
			Debug.LogAlways("shoot");
		}

		Attachment Turret { get; set; }
		Vec3 VelocityRequest { get; set; }

		static float tankTurretMinAngle = -180;
		static float tankTurretMaxAngle = 180;

		static float tankTurretTurnSpeed = 250;
	}
}
