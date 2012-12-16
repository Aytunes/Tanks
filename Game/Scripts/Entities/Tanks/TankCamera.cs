using CryEngine;

namespace CryGameCode.Tanks
{
	public partial class Tank
	{
		protected override void UpdateView(ref ViewParams viewParams)
		{
            if (m_tankInput != null)
            {
                if (m_tankInput.HasFlag(InputFlags.ZoomOut) && ZoomLevel > 1)
                {
                    ZoomLevel -= GameCVars.zoomSpeed * Time.DeltaTime;
                    if (ZoomLevel < 1)
                        ZoomLevel = 1;
                }
                else if (m_tankInput.HasFlag(InputFlags.ZoomIn) && ZoomLevel < GameCVars.maxZoomLevel)
                {
                    ZoomLevel += GameCVars.zoomSpeed *Time.DeltaTime;
                    if (ZoomLevel > GameCVars.maxZoomLevel)
                        ZoomLevel = GameCVars.maxZoomLevel;
                }
            }

            viewParams.FieldOfView = MathHelpers.DegreesToRadians(60);

            var distZ = GameCVars.minCameraDistanceZ + (GameCVars.minCameraDistanceZ - GameCVars.maxCameraDistanceZ) * ZoomRatio;

			if(IsSpectating)
			{
				viewParams.Position = Position + Rotation * new Vec3(0, -10, 5);
				viewParams.Rotation = Quat.CreateRotationVDir(Rotation.Column1);
			}
			else
			{
                viewParams.Position = Position + new Vec3(0, GameCVars.cameraDistanceY, distZ);
                viewParams.Rotation = Quat.CreateRotationXYZ(new Vec3(MathHelpers.DegreesToRadians(GameCVars.minCameraAngleX + (GameCVars.minCameraAngleX - GameCVars.maxCameraAngleX) * ZoomRatio), 0, 0));
			}
		}

		float ZoomLevel;
        float ZoomRatio { get { return ZoomLevel / GameCVars.maxZoomLevel; } }

		public bool IsSpectating { get; set; }
	}
}
