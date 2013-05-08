using CryEngine;
using CryGameCode.Entities.Markers;

namespace CryGameCode.Tanks
{
	enum CameraType
	{
		First = -1,

		TopDown,
		TopDown3D,

		Tilted,

		FirstPerson,

		None,

		Last
	}

	public partial class Tank
	{
		public OverviewPoint OverviewCamera { get; private set; }

		protected override void UpdateView(ref ViewParams viewParams)
		{
			viewParams.FieldOfView = MathHelpers.DegreesToRadians(60);

			if (IsDead)
			{
				viewParams.Position = Position;
				viewParams.Rotation = Rotation;

				return;
			}

			if (Turret == null || !Turret.IsActive)
				return;

			if (Input != null)
			{
				if (Input.HasFlag(InputFlags.ZoomOut) && ZoomLevel > 1)
				{

					ZoomLevel -= GameCVars.cam_zoomSpeed;
					if (ZoomLevel < 1)
						ZoomLevel = 1;
				}
				else if (Input.HasFlag(InputFlags.ZoomIn) && ZoomLevel < GameCVars.cam_maxZoomLevel)
				{
					ZoomLevel += GameCVars.cam_zoomSpeed;
					if (ZoomLevel > GameCVars.cam_maxZoomLevel)
						ZoomLevel = GameCVars.cam_maxZoomLevel;
				}
			}

			if (OverviewCamera != null && OverviewCamera.Active)
			{
				ViewOverviewCamera(ref viewParams);
			}
			else
			{
				switch ((CameraType)GameCVars.cam_type)
				{
					case CameraType.TopDown:
						ViewTopDownCamera(ref viewParams);
						break;
					case CameraType.TopDown3D:
						ViewTopDown3DCamera(ref viewParams);
						break;
					case CameraType.FirstPerson:
						ViewFirstPerson(ref viewParams);
						break;
					case CameraType.Tilted:
						ViewTiltedCamera(ref viewParams);
						break;
				}
			}
		}

		void ViewOverviewCamera(ref ViewParams viewParams)
		{
			viewParams.Position = Vec3.CreateLerp(viewParams.Position, OverviewCamera.Position, Time.DeltaTime * OverviewPoint.MoveLerpSpeed);
			viewParams.Rotation = Quat.CreateNlerp(viewParams.Rotation, OverviewCamera.Rotation, Time.DeltaTime * OverviewPoint.RotationLerpSpeed);
		}

		void ViewTopDownCamera(ref ViewParams viewParams)
		{
			var distZ = GameCVars.cam_topDown_minDistZ + (GameCVars.cam_topDown_minDistZ - GameCVars.cam_topDown_maxDistZ) * ZoomRatio;

			var position = Vec3.CreateLerp(viewParams.PositionLast, Position + new Vec3(0, GameCVars.cam_topDown_distY, distZ), Time.DeltaTime * GameCVars.cam_topDown_posInterpolationSpeed);
			if (position.IsValid)
				viewParams.Position = position;

			var goalRotation = Quat.CreateRotationXYZ(new Vec3(MathHelpers.DegreesToRadians(GameCVars.cam_topDown_minAngleX + (GameCVars.cam_topDown_minAngleX - GameCVars.cam_topDown_maxAngleX) * ZoomRatio), 0, 0));

			viewParams.Rotation = Quat.CreateNlerp(viewParams.RotationLast, goalRotation, Time.DeltaTime * GameCVars.cam_topDown_rotInterpolationSpeed);
		}

		void ViewTiltedCamera(ref ViewParams viewParams)
		{
			var distZ = GameCVars.cam_tilted_minDistZ + (GameCVars.cam_tilted_minDistZ - GameCVars.cam_tilted_maxDistZ) * ZoomRatio;

			var position = Vec3.CreateLerp(viewParams.PositionLast, Position + new Vec3(0, GameCVars.cam_tilted_distY, distZ), Time.DeltaTime * GameCVars.cam_topDown_posInterpolationSpeed);
			if (position.IsValid)
				viewParams.Position = position;

			var goalRotation = Quat.CreateRotationXYZ(new Vec3(MathHelpers.DegreesToRadians(GameCVars.cam_tilted_minAngleX + (GameCVars.cam_tilted_minAngleX - GameCVars.cam_tilted_maxAngleX) * ZoomRatio), 0, 0));

			viewParams.Rotation = Quat.CreateNlerp(viewParams.RotationLast, goalRotation, Time.DeltaTime * GameCVars.cam_tilted_rotInterpolationSpeed);
		}

		void ViewTopDown3DCamera(ref ViewParams viewParams)
		{
			var distZ = GameCVars.cam_topDown3D_minDistZ + (GameCVars.cam_topDown3D_minDistZ - GameCVars.cam_topDown3D_maxDistZ) * ZoomRatio;

			viewParams.Position = viewParams.PositionLast;

			var desiredPosition = Position + GroundNormal * distZ;
			var delta = viewParams.Position - desiredPosition;

			MathHelpers.Interpolate(ref viewParams.Position, desiredPosition, GameCVars.cam_topDown3D_posInterpolationSpeed * Time.DeltaTime);

			if (delta.Length < 0.2f)
				return;

			var desiredRotation = Quat.CreateRotationVDir((Position - viewParams.Position).Normalized);

			viewParams.Rotation = Quat.CreateSlerp(viewParams.RotationLast, desiredRotation, GameCVars.cam_topDown3D_rotInterpolationSpeed * Time.DeltaTime);
		}

		void ViewFirstPerson(ref ViewParams viewParams)
		{
			viewParams.Rotation = Turret.TurretEntity.Rotation;
			viewParams.Position = Turret.TurretEntity.Position + viewParams.Rotation * new Vec3(0, -5, 1.5f);
		}

		float ZoomLevel;
		float ZoomRatio { get { return ZoomLevel / GameCVars.cam_maxZoomLevel; } }
	}
}
