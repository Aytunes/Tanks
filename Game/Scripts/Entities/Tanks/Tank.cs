using CryEngine;
using CryGameCode.Entities;
using CryGameCode.Projectiles;

namespace CryGameCode.Tanks
{
	public abstract class Tank : DamageableActor
	{
		static Tank()
		{
			CVar.RegisterFloat("g_tankTurnSpeed", ref tankTurnSpeed);

			CVar.RegisterFloat("cam_minDistZ", ref minCameraDistanceZ);
			CVar.RegisterFloat("cam_maxDistZ", ref maxCameraDistanceZ);

			CVar.RegisterFloat("cam_distY", ref cameraDistanceY);

			CVar.RegisterFloat("cam_minAngleX", ref minCameraAngleX);
			CVar.RegisterFloat("cam_maxAngleX", ref maxCameraAngleX);

			CVar.RegisterFloat("cam_zoomSpeed", ref zoomSpeed);

			ConsoleCommand.Register("spawn", (e) =>
			{
				//Entity.Spawn<AutocannonTank>("spawnedTank", (Actor.LocalClient as CameraProxy).TargetEntity.Position);
			});
		}

		public override void OnSpawn()
		{
			ZoomLevel = 1;

			Input.ActionmapEvents.Add("zoom_in", OnZoomIn);
			Input.ActionmapEvents.Add("zoom_out", OnZoomOut);

			Input.ActionmapEvents.Add("moveright", OnMoveRight);
			Input.ActionmapEvents.Add("moveleft", OnMoveLeft);
			Input.ActionmapEvents.Add("moveforward", OnMoveForward);
			Input.ActionmapEvents.Add("moveback", OnMoveBack);
			Input.ActionmapEvents.Add("sprint", OnSprint);

			// Temp hax for right mouse events not working
			Input.ActionmapEvents.Add("attack2", (e) =>
			{
				switch (e.KeyEvent)
				{
					case KeyEvent.OnPress:
						if (AutomaticFire)
							m_rightFiring = true;
						break;

					case KeyEvent.OnRelease:
						if (AutomaticFire)
							m_rightFiring = false;
						else
							FireRight();
						break;
				}
			});

			Input.MouseEvents += ProcessMouseEvents;

			OnDestroyed += (e) => 
			{
				Input.MouseEvents -= ProcessMouseEvents;
				Input.ActionmapEvents.RemoveAll(this);
			};

			Reset();
		}

		protected override void OnEditorReset(bool enteringGame)
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

			LeftTrack = GetAttachment("track_left");
			RightTrack = GetAttachment("track_right");

			// Unhide just in case
			Hide(false);

			Physics.AutoUpdate = false;
			Physics.Type = PhysicalizationType.Living;
			Physics.Mass = 500;
			Physics.HeightCollider = 1.2f;
			Physics.Slot = 0;
			Physics.UseCapsule = false;
			Physics.SizeCollider = new Vec3(2.2f, 2.2f, 0.2f);
			Physics.FlagsOR = PhysicalizationFlags.MonitorPostStep;
			Physics.Save();

			if(AutomaticFire)
				ReceiveUpdates = true;

			InitHealth(100);
		}

		public override void OnDeath()
		{
			Debug.DrawText("Died!", 3, Color.Red, 5);

			// Don't remove tank if it was placed by hand via the Editor.
			if (Flags.HasFlag(EntityFlags.NoSave))
				Remove();
			else
				Hide(true);
		}

		public override void OnDamage(float damage, DamageType type)
		{
			Debug.DrawText(string.Format("Took {0} points of {1} damage", damage, type), 3, Color.White, 3);

			if (OnDamaged != null)
				OnDamaged(damage, type);
		}

		void Hide(bool hide)
		{
			Hidden = hide;

			if(!Turret.IsDestroyed)
				Turret.Hidden = hide;

			if(!LeftTrack.IsDestroyed)
				LeftTrack.Hidden = hide;

			if(!RightTrack.IsDestroyed)
				RightTrack.Hidden = hide;
		}

		public delegate void OnDamagedDelegate(float damage, DamageType type);
		public event OnDamagedDelegate OnDamaged;

		#region Movement
		protected override void OnPrePhysicsUpdate()
		{
			var moveRequest = new EntityMovementRequest();
			moveRequest.type = EntityMoveType.Normal;

			if (!Physics.LivingStatus.IsFlying)
				moveRequest.velocity = VelocityRequest;

			moveRequest.rotation = LocalRotation;
			moveRequest.rotation.SetRotationXYZ(RotationRequest * Time.DeltaTime);
			moveRequest.rotation = moveRequest.rotation.Normalized;

			AddMovement(ref moveRequest);

			VelocityRequest = Vec3.Zero;
			RotationRequest = Vec3.Zero;

			if (moveRequest.velocity != Vec3.Zero)
			{
				var moveMat = Material.Find("objects/tanks/tracksmoving");
				if (moveMat != null && !LeftTrack.IsDestroyed && !RightTrack.IsDestroyed)
				{
					LeftTrack.Material = moveMat;
					RightTrack.Material = moveMat;
				}
			}
			else
			{
				var defaultMat = Material.Find("objects/tanks/tracks");
				if (defaultMat != null && !LeftTrack.IsDestroyed && !RightTrack.IsDestroyed)
				{
					LeftTrack.Material = defaultMat;
					RightTrack.Material = defaultMat;
				}
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
			if(e.KeyEvent == KeyEvent.OnPress)
				SpeedMultiplier = 1.5f;
			else if(e.KeyEvent == KeyEvent.OnRelease)
				SpeedMultiplier = 1;
		}

		protected Vec3 VelocityRequest;
		protected Vec3 RotationRequest;

		static float tankTurnSpeed = 2;

		public float SpeedMultiplier = 1.0f;
		public virtual float TankSpeed { get { return 10; } }
		#endregion

		#region Camera
		protected override void UpdateView(ref ViewParams viewParams)
		{
			if (zoomingOut && ZoomLevel > 1)
			{
				ZoomLevel -= zoomSpeed;
				if (ZoomLevel < 1)
					ZoomLevel = 1;
			}
			else if (zoomingIn && ZoomLevel < maxZoomLevel)
			{
				ZoomLevel += zoomSpeed;
				if (ZoomLevel > maxZoomLevel)
					ZoomLevel = maxZoomLevel;
			}

			viewParams.FieldOfView = Math.DegreesToRadians(60);

			var distZ = minCameraDistanceZ + (minCameraDistanceZ - maxCameraDistanceZ) * ZoomRatio;

			if (IsSpectating)
			{
				viewParams.Position = Position + Rotation * new Vec3(0, -10, 5);
				viewParams.Rotation = Quat.CreateRotationVDir(Rotation.Column1);
			}
			else
			{
				viewParams.Position = Position + new Vec3(0, cameraDistanceY, distZ);
				viewParams.Rotation = Quat.CreateRotationXYZ(new Vec3(Math.DegreesToRadians(minCameraAngleX + (minCameraAngleX - maxCameraAngleX) * ZoomRatio), 0, 0));
			}
		}

		private void OnZoomIn(ActionMapEventArgs e)
		{
			if (e.KeyEvent == KeyEvent.OnPress)
				zoomingIn = true;
			else if (e.KeyEvent == KeyEvent.OnRelease)
				zoomingIn = false;
		}

		private void OnZoomOut(ActionMapEventArgs e)
		{
			if (e.KeyEvent == KeyEvent.OnPress)
				zoomingOut = true;
			else if (e.KeyEvent == KeyEvent.OnRelease)
				zoomingOut = false;
		}

		bool zoomingIn;
		bool zoomingOut;

		float ZoomLevel;
		float ZoomRatio { get { return ZoomLevel / maxZoomLevel; } }

		public bool IsSpectating { get; set; }

		public static float minCameraDistanceZ = 25;
		public static float maxCameraDistanceZ = 35;

		public static float cameraDistanceY = -5;

		public static float minCameraAngleX = -80;
		public static float maxCameraAngleX = -90;

		public static float zoomSpeed = 2;

		public static int maxZoomLevel = 8;
		#endregion

		string team;
		[EditorProperty]
		public string Team
		{
			get { return team ?? "red"; }
			set
			{
				if((GameRules.Current as SinglePlayer).IsTeamValid(value))
				{
					team = value;
				}
			}
		}

		public string Model { get { return "objects/tanks/tank_generic_" + Team + ".cdf"; } }

		#region Weaponry
		public override void OnUpdate()
		{
			if (m_leftFiring)
				FireLeft();

			if (m_rightFiring)
				FireRight();
		}

		private void ProcessMouseEvents(MouseEventArgs e)
		{
			switch (e.MouseEvent)
			{
				// Handle turret rotation
				case MouseEvent.Move:
					{
						m_mousePos = Renderer.ScreenToWorld(e.X, e.Y);

						var dir = m_mousePos - Turret.Position;

						var rot = Turret.Rotation;
						rot.SetRotationZ(Math.Atan2(-dir.X, dir.Y));
						Turret.Rotation = rot;
					}
					break;

				case MouseEvent.LeftButtonDown:
					{
						if (AutomaticFire)
							m_leftFiring = true;

						ChargeWeapon();
					}
					break;

				case MouseEvent.LeftButtonUp:
					{
						if (AutomaticFire)
							m_leftFiring = false;
						else
							FireLeft();
					}
					break;
			}
		}

		protected virtual void ChargeWeapon() { }

		private void Fire(ref float shotTime, string helper)
		{
			if(Time.FrameStartTime > shotTime + (TimeBetweenShots * 1000))
			{
				shotTime = Time.FrameStartTime;

				var jointAbsolute = Turret.GetJointAbsolute(helper);
				jointAbsolute.T = Turret.Transform.TransformPoint(jointAbsolute.T);
				Entity.Spawn("pain", ProjectileType, jointAbsolute.T, Turret.Rotation);
				OnFire(jointAbsolute.T);
			}
		}

		protected void FireLeft()
		{
			Fire(ref m_lastleftShot, LeftHelper);
		}

		protected void FireRight()
		{
			if(!string.IsNullOrEmpty(RightHelper))
				Fire(ref m_lastRightShot, RightHelper);
		}

		protected virtual void OnFire(Vec3 firePos) { }

		public virtual bool AutomaticFire { get { return false; } }
		public virtual float TimeBetweenShots { get { return 1; } }

		public virtual System.Type ProjectileType { get { return typeof(Bullet); } }

		private float m_lastleftShot;
		private float m_lastRightShot;
		private bool m_rightFiring;
		private bool m_leftFiring;
		#endregion

		public abstract string TurretModel { get; }
		public virtual string LeftHelper { get { return "turret_term"; } }
		public virtual string RightHelper { get { return string.Empty; } }

		private Vec3 m_mousePos;

		protected Attachment Turret { get; set; }
		protected Attachment LeftTrack { get; set; }
		protected Attachment RightTrack { get; set; }
	}
}