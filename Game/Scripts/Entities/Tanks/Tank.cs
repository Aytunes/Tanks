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
			var request = new EntityMovementRequest();
			AddMovement(ref request);

			Reset();
		}
		
		public abstract float TankSpeed
		{
			get;
		}
		
		public float SpeedMultiplier = 1.0f;
		
		protected override void OnReset(bool enteringGame)
		{
			Reset();
		}

		void Reset()
		{
			LoadObject(Model);

			Turret = GetAttachment("turret");
			Turret.UseEntityRotation = true; // We want to be able to independently rotate it

			Turret.LoadObject(TurretModel);
			Turret.Material = Material.Find("objects/tanks/tank_turrets_" + Team);

			Physicalize(Physics);
			//Physicalize(Turret.Physics);
		}

		void Physicalize(EntityPhysics physics)
		{
			physics.AutoUpdate = false;
			physics.Type = PhysicalizationType.Living;
			physics.Mass = 500;
			physics.HeightCollider = 1.2f;
			physics.UseCapsule = false;
			physics.SizeCollider = new Vec3(2.2f, 2.2f, 0.2f);
			physics.Save();
		}

		protected override bool OnRemove()
		{
			Input.ActionmapEvents.RemoveAll(this);
			Input.MouseEvents -= ProcessMouseEvents;

			return true;
		}

		protected override void OnPrePhysicsUpdate()
		{
			var leftTrack = GetAttachment("track_left");
			var rightTrack = GetAttachment("track_right");

			if (VelocityRequest != Vec3.Zero)
			{
				var moveMat = Material.Find("objects/tanks/tracksmoving");
				if (moveMat != null)
				{
					leftTrack.Material = moveMat;
					rightTrack.Material = moveMat;
				}
			}
			else
			{
				var defaultMat = Material.Find("objects/tanks/tracks");
				if (defaultMat != null)
				{
					leftTrack.Material = defaultMat;
					rightTrack.Material = defaultMat;
				}
			}

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
			VelocityRequest += LocalRotation.Column1 * TankSpeed * SpeedMultiplier;
		}

		private void OnMoveBack(ActionMapEventArgs e)
		{
			VelocityRequest += LocalRotation.Column1 * -TankSpeed * SpeedMultiplier;
		}
		
		private void OnSprint(ActionMapEventArgs e)
		{
			if (e.KeyEvent == KeyEvent.OnPress)
			SpeedMultiplier = 1.5f;
			else if (e.KeyEvent == KeyEvent.OnRelease)
			SpeedMultiplier = 1;
		}
		
		string team;
		[EditorProperty]
		public string Team 
		{
			get { return team ?? "red"; }
			set
			{
				if ((GameRules.Current as SinglePlayer).IsTeamValid(value))
				{
					team = value;
					Reset();
				}
			}
		}

		public string Model { get { return "objects/tanks/tank_generic_" + Team + ".cdf"; } }
		public abstract string TurretModel { get; }


		public virtual void ChargeWeapon() { }
		public abstract void FireWeapon(Vec3 mouseWorldPos);

		EntityBase owner;
		public EntityBase Owner
		{
			get { return owner; }
			set
			{
				owner = value;

				Input.ActionmapEvents.Add("moveright", OnMoveRight);
				Input.ActionmapEvents.Add("moveleft", OnMoveLeft);
				Input.ActionmapEvents.Add("moveforward", OnMoveForward);
				Input.ActionmapEvents.Add("moveback", OnMoveBack);
				Input.ActionmapEvents.Add("sprint", OnSprint);

				Input.MouseEvents += ProcessMouseEvents;
			}
		}

		protected Attachment Turret { get; set; }

		protected Vec3 VelocityRequest;
		protected Vec3 RotationRequest;

		static float tankTurretMinAngle = -180;
		static float tankTurretMaxAngle = 180;

		static float tankTurretTurnSpeed = 250;

		static float tankTurnSpeed = 2;
	}
}
