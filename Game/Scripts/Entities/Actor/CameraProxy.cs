using CryEngine;

namespace CryGameCode.Entities
{
	public class CameraProxy : Actor
	{
		static CameraProxy()
		{
			CVar.RegisterFloat("cam_distZ", ref cameraDistanceZ);
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
				viewParams.Rotation = Quat.CreateRotationXYZ(new Vec3(Math.DegreesToRadians(-90), 0, 0));
			}
		}

		public static float cameraDistanceZ = 20;

		public Entity TargetEntity { get; set; }
	}
}