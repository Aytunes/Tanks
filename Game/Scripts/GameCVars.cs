using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CryEngine;
using CryEngine.Extensions;

namespace CryGameCode.Tanks
{
    public static class GameCVars
	{
        static GameCVars()
		{
			CVar.RegisterFloat("cam_minDistZ", ref minCameraDistanceZ);
			CVar.RegisterFloat("cam_maxDistZ", ref maxCameraDistanceZ);

			CVar.RegisterFloat("cam_distY", ref cameraDistanceY);

			CVar.RegisterFloat("cam_minAngleX", ref minCameraAngleX);
			CVar.RegisterFloat("cam_maxAngleX", ref maxCameraAngleX);

			CVar.RegisterFloat("cam_zoomSpeed", ref zoomSpeed);

            CVar.RegisterFloat("tank_turnModifier", ref turnModifier);

            CVar.RegisterFloat("tank_movementSpeedMult", ref movementSpeedMultiplier);

            CVar.RegisterInt("g_hardcoreMode", ref hardcoreMode);

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

        #region Camera
        public static float minCameraDistanceZ = 25;
		public static float maxCameraDistanceZ = 40;

		public static float cameraDistanceY = -5;

		public static float minCameraAngleX = -80;
		public static float maxCameraAngleX = -90;

		public static float zoomSpeed = 2;

		public static int maxZoomLevel = 8;
        #endregion

    #region Tank movement
        public static float turnModifier = 1.2f;

        public static float movementSpeedMultiplier = 6.0f;
    #endregion

        public static int hardcoreMode;

        public static string ForceTankType;
        public static List<System.Type> TurretTypes;
	}
}
