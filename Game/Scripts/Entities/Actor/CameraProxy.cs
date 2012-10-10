using CryEngine;

namespace CryGameCode.Entities
{
	public class CameraProxy : Actor
	{
		static CameraProxy()
		{
			CVar.RegisterFloat("cam_minDistZ", ref minCameraDistanceZ);
			CVar.RegisterFloat("cam_maxDistZ", ref maxCameraDistanceZ);

			CVar.RegisterFloat("cam_distY", ref cameraDistanceY);

			CVar.RegisterFloat("cam_minAngleX", ref minCameraAngleX);
			CVar.RegisterFloat("cam_maxAngleX", ref maxCameraAngleX);

			CVar.RegisterFloat("cam_zoomSpeed", ref zoomSpeed);
		}

		public void Init()
		{
			Input.ActionmapEvents.Add("zoom_in", OnZoomIn);
			Input.ActionmapEvents.Add("zoom_out", OnZoomOut);

			ZoomLevel = 1;

			Position = TargetEntity.Position;
		}

		public override void UpdateView(ref ViewParams viewParams)
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

			if (TargetEntity != null && !TargetEntity.IsDestroyed)
			{
				var distZ = minCameraDistanceZ + (minCameraDistanceZ - maxCameraDistanceZ) * ZoomRatio;

				if (IsSpectating)
				{
					viewParams.Position = TargetEntity.Position + TargetEntity.Rotation * new Vec3(0, -10, 5);
					viewParams.Rotation = Quat.CreateRotationVDir(TargetEntity.Rotation.Column1);
				}
				else
				{
					viewParams.Position = TargetEntity.Position +  new Vec3(0, cameraDistanceY, distZ);
					viewParams.Rotation = Quat.CreateRotationXYZ(new Vec3(Math.DegreesToRadians(minCameraAngleX + (minCameraAngleX - maxCameraAngleX) * ZoomRatio), 0, 0));
				}
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
		public Entity TargetEntity { get; set; }

		public static float minCameraDistanceZ = 25;
		public static float maxCameraDistanceZ = 35;

		public static float cameraDistanceY = -5;

		public static float minCameraAngleX = -80;
		public static float maxCameraAngleX = -90;

		public static float zoomSpeed = 2;

		public static int maxZoomLevel = 8;
	}
}