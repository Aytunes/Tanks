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

			Input.ActionmapEvents.Add("moveright", OnMoveRight);
			Input.ActionmapEvents.Add("moveleft", OnMoveLeft);
			Input.ActionmapEvents.Add("moveforward", OnMoveForward);
			Input.ActionmapEvents.Add("moveback", OnMoveBack);

			Input.MouseEvents += ProcessMouseEvents;
		}

		protected override bool OnRemove()
		{
			Input.ActionmapEvents.RemoveAll(this);
			Input.MouseEvents -= ProcessMouseEvents;

			return true;
		}

		protected override void OnPrePhysicsUpdate()
		{
			var entityRot = Rotation.Normalized;

			var moveRequest = new EntityMovementRequest();
			moveRequest.type = EntityMoveType.Normal;
			moveRequest.velocity = VelocityRequest;

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

						var rot = Turret.Rotation;
						rot.SetRotationZ(Math.Atan2(-dir.X, dir.Y));
						Turret.Rotation = rot;
					}
					break;
				case MouseEvent.LeftButtonDown:
					ChargeWeapon();
					break;
				case MouseEvent.LeftButtonUp:
					{
						var mousePos = Renderer.ScreenToWorld(e.X, e.Y);
						FireWeapon(mousePos);
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

		public string Model { get { return "objects/tanks/tank_generic.cdf"; } }
		public abstract string TurretModel { get; }

		public virtual void ChargeWeapon() { }
		public abstract void FireWeapon(Vec3 mouseWorldPos);

		protected Attachment Turret { get; set; }
		protected Vec3 VelocityRequest;
		protected Vec3 RotationRequest;

		static float tankTurretMinAngle = -180;
		static float tankTurretMaxAngle = 180;

		static float tankTurretTurnSpeed = 250;

		static float tankTurnSpeed = 2;
	}
}
