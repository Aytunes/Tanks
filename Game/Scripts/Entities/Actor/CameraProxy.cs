using CryEngine;

namespace CryGameCode.Entities
{
	public class CameraProxy : Actor
	{
		static CameraProxy()
		{
			CVar.RegisterFloat("cam_minDistZ", ref minCameraDistanceZ);
			CVar.RegisterFloat("cam_maxDistZ", ref maxCameraDistanceZ);
			CVar.RegisterFloat("cam_minAngleX", ref minCameraAngleX);
			CVar.RegisterFloat("cam_maxAngleX", ref maxCameraAngleX);
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
			viewParams.FieldOfView = Math.DegreesToRadians(60);

			if (TargetEntity != null && !TargetEntity.IsDestroyed)
			{
				var distZ = minCameraDistanceZ + (minCameraDistanceZ - maxCameraDistanceZ) * ZoomRatio;

				viewParams.Position = TargetEntity.Position + new Vec3(0, 0, distZ);
				viewParams.Rotation = Quat.CreateRotationXYZ(new Vec3(Math.DegreesToRadians(minCameraAngleX + (minCameraAngleX - maxCameraAngleX) * ZoomRatio), 0, 0));
			}
		}

		private void OnZoomIn(ActionMapEventArgs e)
		{
			if (ZoomLevel < maxZoomLevel)
				ZoomLevel++;
		}

		private void OnZoomOut(ActionMapEventArgs e)
		{
			if (ZoomLevel > 1)
				ZoomLevel--;
		}

		int ZoomLevel;
		float ZoomRatio { get { return ZoomLevel / (float)maxZoomLevel; } }

		public Entity TargetEntity { get; set; }

		public static float minCameraDistanceZ = 25;
		public static float maxCameraDistanceZ = 35;
		public static float minCameraAngleX = -55;
		public static float maxCameraAngleX = -65;

		public static int maxZoomLevel = 8;
	}
}