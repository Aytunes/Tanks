using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using CryEngine;
using CryEngine.Extensions;
using CryGameCode.Entities;
using CryGameCode.Projectiles;

namespace CryGameCode.Tanks
{
	public class Tank : DamageableActor
	{
		#region Statics
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

			TurretTypes = (from type in Assembly.GetExecutingAssembly().GetTypes()
						   where type.Implements<TankTurret>()
						   select type).ToList();

			ConsoleCommand.Register("SetTankType", (e) =>
			{
				ForceTankType = e.Args[0];
			});

			ConsoleCommand.Register("ResetTankType", (e) =>
			{
				ForceTankType = string.Empty;
			});
		}

		#region CVars
		public static float minCameraDistanceZ = 25;
		public static float maxCameraDistanceZ = 35;

		public static float cameraDistanceY = -5;

		public static float minCameraAngleX = -80;
		public static float maxCameraAngleX = -90;

		public static float zoomSpeed = 2;

		public static int maxZoomLevel = 8;
		#endregion

		private static string ForceTankType;

		private static List<System.Type> TurretTypes;
		#endregion

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

			OnDestroyed += (e) => 
			{
				Input.ActionmapEvents.RemoveAll(this);

				if (Turret != null)
				{
					Turret.Destroy();
					Turret = null;
				}
			};

			Reset(true);
		}

		protected override void OnEditorReset(bool enteringGame)
		{
			Reset(enteringGame);
		}

		void Reset(bool enteringGame)
		{
			LoadObject(Model);

			if (enteringGame)
			{
				System.Type turretType;

				if (string.IsNullOrEmpty(ForceTankType))
					turretType = TurretTypes[SinglePlayer.Selector.Next(TurretTypes.Count)];
				else
					turretType = System.Type.GetType("CryGameCode.Tanks." + ForceTankType, true, true);

				Turret = System.Activator.CreateInstance(turretType, this) as TankTurret;
			}
			else
			{
				Turret.Destroy();
				Turret = null;
			}

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

			InitHealth(100);

			ReceiveUpdates = true;
		}

		public override void OnUpdate()
		{
			if(Turret != null)
				Turret.Update();
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

			if(Turret != null)
				Turret.Hide(hide);

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
			if (IsDestroyed)
				return;

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
		#endregion

		string team;
		[EditorProperty]
		public string Team
		{ 
			get { return team ?? "red"; }
			set
			{
				var gameRules = GameRules.Current as SinglePlayer;
				if (gameRules != null && gameRules.IsTeamValid(value))
				{
					team = value;
				}
			}
		}

		public string Model { get { return "objects/tanks/tank_generic_" + Team + ".cdf"; } }

		protected TankTurret Turret { get; set; }
		protected Attachment LeftTrack { get; set; }
		protected Attachment RightTrack { get; set; }
	}
}
