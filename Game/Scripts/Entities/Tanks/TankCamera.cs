using CryEngine;

namespace CryGameCode.Tanks
{
    enum CameraType
    {
        First = -1,

        TopDown,
        FirstPerson,
        None,

        Last
    }

	public partial class Tank
	{
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

            if (m_playerInput != null)
            {
                if (m_playerInput.HasFlag(InputFlags.ZoomOut) && ZoomLevel > 1)
                {

                    ZoomLevel -= GameCVars.cam_zoomSpeed;
                    if (ZoomLevel < 1)
                        ZoomLevel = 1;
                }
                else if (m_playerInput.HasFlag(InputFlags.ZoomIn) && ZoomLevel < GameCVars.cam_maxZoomLevel)
                {
                    ZoomLevel += GameCVars.cam_zoomSpeed;
                    if (ZoomLevel > GameCVars.cam_maxZoomLevel)
                        ZoomLevel = GameCVars.cam_maxZoomLevel;
                }
            }

            switch ((CameraType)GameCVars.cam_type)
            {
                case CameraType.TopDown:
                    ViewTopDownCamera(ref viewParams);
                    break;
                case CameraType.FirstPerson:
                    ViewFirstPerson(ref viewParams);
                    break;
            }
		}

        void ViewTopDownCamera(ref ViewParams viewParams)
        {
            var distZ = GameCVars.cam_minDistZ + (GameCVars.cam_minDistZ - GameCVars.cam_maxDistZ) * ZoomRatio;

            viewParams.Position = Turret.Attachment.Position + new Vec3(0, GameCVars.cam_distY, distZ);
            viewParams.Rotation = Quat.CreateRotationXYZ(new Vec3(MathHelpers.DegreesToRadians(GameCVars.cam_minAngleX + (GameCVars.cam_minAngleX - GameCVars.cam_maxAngleX) * ZoomRatio), 0, 0));
        }

        void ViewFirstPerson(ref ViewParams viewParams)
        {
            viewParams.Rotation = Turret.Attachment.Rotation;
            viewParams.Position = Turret.Attachment.Position + viewParams.Rotation * new Vec3(0, -5, 1.5f);
        }

		float ZoomLevel;
        float ZoomRatio { get { return ZoomLevel / GameCVars.cam_maxZoomLevel; } }
	}
}
