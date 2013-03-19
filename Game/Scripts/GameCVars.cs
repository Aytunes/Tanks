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
			RegisterCameraCVars();
			RegisterTankMovementCVars();

			RegisterConsoleCommands();
		}

		#region Camera
		static void RegisterCameraCVars()
		{
			CVar.RegisterFloat("cam_posInterpolationSpeed", ref cam_posInterpolationSpeed);

			CVar.RegisterFloat("cam_minDistZ", ref cam_minDistZ);
			CVar.RegisterFloat("cam_maxDistZ", ref cam_maxDistZ);

			CVar.RegisterFloat("cam_distY", ref cam_distY);

			CVar.RegisterFloat("cam_minAngleX", ref cam_minAngleX);
			CVar.RegisterFloat("cam_maxAngleX", ref cam_maxAngleX);

			CVar.RegisterFloat("cam_zoomSpeed", ref cam_zoomSpeed);
			CVar.RegisterInt("cam_maxZoomLevel", ref cam_maxZoomLevel);

			CVar.RegisterInt("cam_type", ref cam_type);
		}

		public static float cam_posInterpolationSpeed = 1000;

		public static float cam_minDistZ = 37;
		public static float cam_maxDistZ = 60;

		public static float cam_distY = -5;

		public static float cam_minAngleX = -80;
		public static float cam_maxAngleX = -90;

		public static float cam_zoomSpeed = 2;

		public static int cam_maxZoomLevel = 8;

		public static int cam_type = (int)CameraType.TopDown;
		#endregion

		#region Tank movement
		static void RegisterTankMovementCVars()
		{
			CVar.RegisterFloat("tank_movementSpeedMult", ref tank_movementSpeedMult);
			CVar.RegisterFloat("tank_movementMaxSpeed", ref tank_movementMaxSpeed);

			CVar.RegisterFloat("tank_rotationSpeed", ref tank_rotationSpeed);

			CVar.RegisterFloat("tank_maxAcceleration", ref tank_maxAcceleration);
			CVar.RegisterFloat("tank_maxAccelerationBoosting", ref tank_maxAcceleration);

			CVar.RegisterFloat("tank_accelerationSpeed", ref tank_accelerationSpeed);
			CVar.RegisterFloat("tank_accelerationSpeedRotation", ref tank_accelerationSpeedRotation);
			CVar.RegisterFloat("tank_decelerationSpeed", ref tank_decelerationSpeed);
		}

		public static float tank_movementSpeedMult = 6.0f;
		public static float tank_movementMaxSpeed = 8.0f;

		public static float tank_rotationSpeed = 3.0f;

		public static float tank_maxAcceleration = 1.25f;
		public static float tank_maxAccelerationBoosting = 1.5f;

		public static float tank_accelerationSpeed = 5.0f;
		public static float tank_accelerationSpeedRotation = 1.0f;
		public static float tank_decelerationSpeed = 500.0f;
		#endregion

		#region Console commands
		static void RegisterConsoleCommands()
		{
			TurretTypes = (from type in Assembly.GetExecutingAssembly().GetTypes()
						   where type.Implements<TankTurret>()
						   select type).ToList();

			ConsoleCommand.Register("SetTurretType", (e) =>
			{
                if(e.Args.Length > 0)
				    ForceTankType = e.Args[0];
			}, "Sets the turret type", CVarFlags.Cheat, true);

			ConsoleCommand.Register("ResetTurretType", (e) =>
			{
				ForceTankType = string.Empty;
            }, "Resets the forced turret type, set via SetTurretType.", CVarFlags.Cheat, true);
		}

		public static string ForceTankType;
		public static List<System.Type> TurretTypes;
		#endregion
	}
}
