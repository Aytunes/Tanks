using CryEngine;
using CryGameCode.Entities.Markers;

namespace CryGameCode.Tanks
{
	enum CameraType
	{
		First = -1,

		TopDown,
		TopDown3D,
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
				}
			}
		}

		void ViewOverviewCamera(ref ViewParams viewParams)
		{
			viewParams.Position = OverviewCamera.Position;
			viewParams.Rotation = OverviewCamera.Rotation;
		}

		void ViewTopDownCamera(ref ViewParams viewParams)
		{
			var distZ = GameCVars.cam_minDistZ + (GameCVars.cam_minDistZ - GameCVars.cam_maxDistZ) * ZoomRatio;

			viewParams.Position = viewParams.PositionLast;
			MathHelpers.Interpolate(ref viewParams.Position, Position + new Vec3(0, GameCVars.cam_distY, distZ), GameCVars.cam_posInterpolationSpeed * Time.DeltaTime);
			viewParams.Rotation = Quat.CreateRotationXYZ(new Vec3(MathHelpers.DegreesToRadians(GameCVars.cam_minAngleX + (GameCVars.cam_minAngleX - GameCVars.cam_maxAngleX) * ZoomRatio), 0, 0));
		}

		void ViewTopDown3DCamera(ref ViewParams viewParams)
		{
			var distZ = GameCVars.cam_minDistZ + (GameCVars.cam_minDistZ - GameCVars.cam_maxDistZ) * ZoomRatio;

			viewParams.Position = viewParams.PositionLast;

			var desiredPosition = Position + GroundNormal * distZ;

			MathHelpers.Interpolate(ref viewParams.Position, desiredPosition, GameCVars.cam_posInterpolationSpeed * Time.DeltaTime);

			var desiredRotation = Quat.CreateRotationVDir((Position - viewParams.Position).Normalized);

			viewParams.Rotation = Quat.CreateSlerp(viewParams.RotationLast, desiredRotation, Time.DeltaTime);
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
