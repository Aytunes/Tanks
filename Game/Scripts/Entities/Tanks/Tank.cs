using CryEngine;
using CryGameCode.Entities;
using System.Linq;
using CryGameCode.Projectiles;

namespace CryGameCode.Tanks
{
	public abstract class Tank : DamageableEntity
	{
		static Tank()
		{
			CVar.RegisterFloat("g_tankTurretMinAngle", ref tankTurretMinAngle);
			CVar.RegisterFloat("g_tankTurretMaxAngle", ref tankTurretMaxAngle);

			CVar.RegisterFloat("g_tankTurretTurnSpeed", ref tankTurretTurnSpeed);
			CVar.RegisterFloat("g_tankTurnSpeed", ref tankTurnSpeed);

			ConsoleCommand.Register("spawn", (e) =>
			{
				Entity.Spawn<AutocannonTank>("spawnedTank", (Actor.LocalClient as CameraProxy).TargetEntity.Position);
			});
		}

		public override void OnSpawn()
		{
			var request = new EntityMovementRequest();
			AddMovement(ref request);
			Reset();
		}

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
			Physics.Save();

			if(AutomaticFire)
				ReceiveUpdates = true;

			InitHealth(100);
		}

		protected override bool OnRemove()
		{
			Input.ActionmapEvents.RemoveAll(this);
			Input.MouseEvents -= ProcessMouseEvents;

			return true;
		}

		protected override void OnPrePhysicsUpdate()
		{
			var moveRequest = new EntityMovementRequest();
			moveRequest.type = EntityMoveType.Normal;

			if(!Physics.LivingStatus.IsFlying)
				moveRequest.velocity = VelocityRequest;

			moveRequest.rotation = LocalRotation;
			moveRequest.rotation.SetRotationXYZ(RotationRequest * Time.DeltaTime);
			moveRequest.rotation = moveRequest.rotation.Normalized;

			AddMovement(ref moveRequest);

			VelocityRequest = Vec3.Zero;
			RotationRequest = Vec3.Zero;

			if(moveRequest.velocity != Vec3.Zero)
			{
				var moveMat = Material.Find("objects/tanks/tracksmoving");
				if(moveMat != null)
				{
					LeftTrack.Material = moveMat;
					RightTrack.Material = moveMat;
				}
			}
			else
			{
				var defaultMat = Material.Find("objects/tanks/tracks");
				if(defaultMat != null)
				{
					LeftTrack.Material = defaultMat;
					RightTrack.Material = defaultMat;
				}
			}
		}

		public override void OnUpdate()
		{
			if(m_leftFiring)
				FireLeft();

			if(m_rightFiring)
				FireRight();
		}

		protected override void OnDeath()
		{
			Debug.DrawText("Died!", 3, Color.Red, 5);

			// Don't remove tank if it was placed by hand via the Editor.
			if (Flags.HasFlag(EntityFlags.NoSave))
				Remove();
			else
				Hide(true);
		}

		void Hide(bool hide)
		{
			Hidden = hide;
			Turret.Hidden = hide;
			LeftTrack.Hidden = hide;
			RightTrack.Hidden = hide;
		}

		protected override void OnDamage(float damage, DamageType type)
		{
			Debug.DrawText(string.Format("Took {0} points of {1} damage", damage, type), 3, Color.White, 3);

			if(OnDamaged != null)
				OnDamaged(damage, type);
		}

		public delegate void OnDamagedDelegate(float damage, DamageType type);
		public event OnDamagedDelegate OnDamaged;

		private void ProcessMouseEvents(MouseEventArgs e)
		{
			switch(e.MouseEvent)
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
						if(AutomaticFire)
							m_leftFiring = true;

						ChargeWeapon();
					}
					break;

				case MouseEvent.LeftButtonUp:
					{
						if(AutomaticFire)
							m_leftFiring = false;
						else
							FireLeft();
					}
					break;

				// TODO: Fix right mouse events
				/*case MouseEvent.RightButtonDown:
					{
						if(AutomaticFire)
							m_rightFiring = true;

						ChargeWeapon();
					}
					break;

				case MouseEvent.RightButtonUp:
					{
						if(AutomaticFire)
							m_rightFiring = false;
						else
				 			FireRight();
					}
					break;*/
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
					Reset();
				}
			}
		}

		public string Model { get { return "objects/tanks/tank_generic_" + Team + ".cdf"; } }
		public float SpeedMultiplier = 1.0f;

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

		public abstract string TurretModel { get; }
		public virtual string LeftHelper { get { return "turret_term"; } }
		public virtual string RightHelper { get { return string.Empty; } }

		public virtual float TankSpeed { get { return 10; } }
		public virtual System.Type ProjectileType { get { return typeof(Bullet); } }

		public virtual bool AutomaticFire { get { return false; } }
		public virtual float TimeBetweenShots { get { return 1; } }

		private float m_lastleftShot;
		private float m_lastRightShot;
		private bool m_rightFiring;
		private bool m_leftFiring;
		private Vec3 m_mousePos;

		Actor owner;
		public Actor Owner
		{
			get { return owner; }
			set
			{
				owner = value;

				if(owner.IsLocalClient)
				{
					Input.ActionmapEvents.Add("moveright", OnMoveRight);
					Input.ActionmapEvents.Add("moveleft", OnMoveLeft);
					Input.ActionmapEvents.Add("moveforward", OnMoveForward);
					Input.ActionmapEvents.Add("moveback", OnMoveBack);
					Input.ActionmapEvents.Add("sprint", OnSprint);

					// Temp hax for right mouse events not working
					Input.ActionmapEvents.Add("attack2", (e) =>
					{
						switch(e.KeyEvent)
						{
							case KeyEvent.OnPress:
								if(AutomaticFire)
									m_rightFiring = true;
								break;

							case KeyEvent.OnRelease:
								if(AutomaticFire)
									m_rightFiring = false;
								else
									FireRight();
								break;
						}
					});

					Input.MouseEvents += ProcessMouseEvents;
				}
			}
		}

		protected Attachment Turret { get; set; }
		protected Attachment LeftTrack { get; set; }
		protected Attachment RightTrack { get; set; }

		protected Vec3 VelocityRequest;
		protected Vec3 RotationRequest;

		static float tankTurretMinAngle = -180;
		static float tankTurretMaxAngle = 180;

		static float tankTurretTurnSpeed = 250;

		static float tankTurnSpeed = 2;
	}
}
