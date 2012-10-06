using CryEngine;

namespace CryGameCode.Entities
{
	public class CameraProxy : Actor
	{
		static CameraProxy()
		{
			CVar.RegisterFloat("cam_distZ", ref cameraDistanceZ);

			CVar.RegisterFloat("cam_angleX", ref cameraAngleX);
			CVar.RegisterFloat("cam_angleY", ref cameraAngleY);
			CVar.RegisterFloat("cam_angleZ", ref cameraAngleZ);
		}

		public void Init()
		{
			Position = TargetEntity.Position;
		}

		public override void UpdateView(ref ViewParams viewParams)
		{
			viewParams.FieldOfView = Math.DegreesToRadians(60);

			if (TargetEntity != null && !TargetEntity.IsDestroyed)
			{
				viewParams.Position = TargetEntity.Position + new Vec3(0, 0, cameraDistanceZ);
				viewParams.Rotation = Quat.CreateRotationXYZ(new Vec3(Math.DegreesToRadians(cameraAngleX), Math.DegreesToRadians(cameraAngleY), Math.DegreesToRadians(cameraAngleZ)));
			}
		}

		public static float cameraDistanceZ = 35;

		public static float cameraAngleX = -80;
		public static float cameraAngleY;
		public static float cameraAngleZ;

		public Entity TargetEntity { get; set; }
	}
}