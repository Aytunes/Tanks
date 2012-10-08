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
				Entity.Spawn<AutocannonTank>("spawnedTank", Entity.GetByClass<HeavyTank>().First().Position);
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

			Physics.AutoUpdate = false;
			Physics.Type = PhysicalizationType.Living;
			Physics.Mass = 500;
			Physics.HeightCollider = 1.2f;
			Physics.Slot = 0;
			Physics.UseCapsule = false;
			Physics.SizeCollider = new Vec3(2.2f, 2.2f, 0.2f);
			Physics.Save();

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

		protected override void OnDeath()
		{
			Debug.DrawText("Died!", 3, Color.Red, 5);
			Remove();
		}

		protected override void OnDamage(float damage, DamageType type)
		{
			Debug.DrawText(string.Format("Took {0} points of {1} damage", damage, type), 3, Color.White, 3);

			OnDamaged(damage, type);
		}

		public delegate void OnDamagedDelegate(float damage, DamageType type);
		public event OnDamagedDelegate OnDamaged;

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
		public float SpeedMultiplier = 1.0f;

		protected virtual void ChargeWeapon() { }
		protected void FireWeapon(Vec3 mouseWorldPos)
		{
			var jointAbsolute = Turret.GetJointAbsolute("turret_term");
			jointAbsolute.T = Turret.Transform.TransformPoint(jointAbsolute.T);

			Entity.Spawn("pain", ProjectileType, jointAbsolute.T, Turret.Rotation);
			
			OnFire(jointAbsolute.T);
		}

		protected virtual void OnFire(Vec3 firePos) { }

		public abstract string TurretModel { get; }
		public virtual float TankSpeed { get { return 10; } }
		public abstract System.Type ProjectileType { get; }

		Actor owner;
		public Actor Owner
		{
			get { return owner; }
			set
			{
				owner = value;

				if (owner.IsLocalClient)
				{
					Input.ActionmapEvents.Add("moveright", OnMoveRight);
					Input.ActionmapEvents.Add("moveleft", OnMoveLeft);
					Input.ActionmapEvents.Add("moveforward", OnMoveForward);
					Input.ActionmapEvents.Add("moveback", OnMoveBack);
					Input.ActionmapEvents.Add("sprint", OnSprint);

					Input.MouseEvents += ProcessMouseEvents;
				}
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
