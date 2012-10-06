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
			CVar.RegisterFloat("g_tankTurnSpeed", ref tankTurnSpeed);
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
			Physics.HeightCollider = 1.2f;
			Physics.UseCapsule = true;
			Physics.SizeCollider = new Vec3(0.4f, 0.4f, 0.2f);
			Physics.Gravity = new Vec3(0, 0, 9.81f);
			Physics.AirControl = 0;
			Physics.MinSlideAngle = 45;
			Physics.MaxClimbAngle = 50;
			Physics.MinFallAngle = 50;
			Physics.MaxVelGround = 16;
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
			var entityRot = Rotation.Normalized;

			var moveRequest = new EntityMovementRequest();
			moveRequest.type = EntityMoveType.Normal;
			moveRequest.velocity = VelocityRequest;

			/*if (moveRequest.velocity != Vec3.Zero)
			{
				Quat qVelocity = Quat.Identity;
				qVelocity.SetRotationVDir(moveRequest.velocity.NormalizedSafe);

				moveRequest.rotation = LocalRotation.Inverted * Quat.CreateSlerp(LocalRotation, qVelocity, Time.DeltaTime);
				moveRequest.rotation = moveRequest.rotation.Normalized;
			}
			else*/
			moveRequest.rotation = LocalRotation;
			moveRequest.rotation.SetRotationXYZ(RotationRequest * Time.DeltaTime);
			moveRequest.rotation = moveRequest.rotation.Normalized;

			AddMovement(ref moveRequest);

			VelocityRequest = Vec3.Zero;
			RotationRequest = Vec3.Zero;
		}

		private void ProcessMouseEvents(MouseEventArgs e)
		{
			switch (e.MouseEvent)
			{
				// Handle turret rotation
				case MouseEvent.Move:
					{
						var mousePos = Renderer.ScreenToWorld(e.X, e.Y);

						Vec3 dir = mousePos - Turret.Position;

						float yaw = Math.RadiansToDegrees(Math.Atan2(dir.Y, dir.X));
						yaw = Math.ClampAngle(yaw, tankTurretMinAngle, tankTurretMaxAngle);
						yaw = Math.DegreesToRadians(yaw);

						var rot = Turret.Rotation;

						rot.SetRotationZ(yaw);
						Turret.Rotation = rot;
					}
					break;
				case MouseEvent.LeftButtonDown:
					{
						var mousePos = Renderer.ScreenToWorld(e.X, e.Y);

						Debug.DrawLine(Turret.Position, mousePos, Color.Red, 2.0f);
					}
					break;
			}
		}

		private void OnMoveRight(ActionMapEventArgs e)
		{
			RotationRequest.Z -= tankTurnSpeed;
		}

		private void OnMoveLeft(ActionMapEventArgs e)
		{
			RotationRequest.Z += tankTurnSpeed;
		}

		private void OnMoveForward(ActionMapEventArgs e)
		{
			VelocityRequest += LocalRotation.Column1 * 10;
		}

		private void OnMoveBack(ActionMapEventArgs e)
		{
			VelocityRequest += LocalRotation.Column1 * -10;
		}

		private void OnShoot(ActionMapEventArgs e)
		{
			Debug.LogAlways("shoot");
		}

		Attachment Turret { get; set; }
		Vec3 VelocityRequest;
		Vec3 RotationRequest;

		static float tankTurretMinAngle = -180;
		static float tankTurretMaxAngle = 180;

		static float tankTurretTurnSpeed = 250;

		static float tankTurnSpeed = 5;
	}
}
