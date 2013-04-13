using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CryEngine;
using CryEngine.Extensions;

using CryGameCode.Entities.Buildings;

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
			RegisterTopDownCameraCVars();
			RegisterTopDown3DCameraCVars();
			RegisterTiltedCameraCVars();
			
			CVar.RegisterFloat("cam_zoomSpeed", ref cam_zoomSpeed);
			CVar.RegisterInt("cam_maxZoomLevel", ref cam_maxZoomLevel);

			CVar.RegisterInt("cam_type", ref cam_type);
		}

		#region Top down camera
		static void RegisterTopDownCameraCVars()
		{
			CVar.RegisterFloat("cam_topDown_posInterpolationSpeed", ref cam_topDown_posInterpolationSpeed);
			CVar.RegisterFloat("cam_topDown_rotInterpolationSpeed", ref cam_topDown_rotInterpolationSpeed);

			CVar.RegisterFloat("cam_topDown_minDistZ", ref cam_topDown_minDistZ);
			CVar.RegisterFloat("cam_topDown_maxDistZ", ref cam_topDown_maxDistZ);

			CVar.RegisterFloat("cam_topDown_distY", ref cam_topDown_distY);

			CVar.RegisterFloat("cam_topDown_minAngleX", ref cam_topDown_minAngleX);
			CVar.RegisterFloat("cam_topDown_maxAngleX", ref cam_topDown_maxAngleX);
		}

		public static float cam_topDown_posInterpolationSpeed = 5;
		public static float cam_topDown_rotInterpolationSpeed = 5;

		public static float cam_topDown_minDistZ = 37;
		public static float cam_topDown_maxDistZ = 60;

		public static float cam_topDown_distY = -5;

		public static float cam_topDown_minAngleX = -80;
		public static float cam_topDown_maxAngleX = -90;
		#endregion

		#region Top down 3D camera
		static void RegisterTopDown3DCameraCVars()
		{
			CVar.RegisterFloat("cam_topDown3D_posInterpolationSpeed", ref cam_topDown3D_posInterpolationSpeed);
			CVar.RegisterFloat("cam_topDown3D_rotInterpolationSpeed", ref cam_topDown3D_rotInterpolationSpeed);

			CVar.RegisterFloat("cam_topDown3D_minDistZ", ref cam_topDown3D_minDistZ);
			CVar.RegisterFloat("cam_topDown3D_maxDistZ", ref cam_topDown3D_maxDistZ);
		}

		public static float cam_topDown3D_posInterpolationSpeed = 100;
		public static float cam_topDown3D_rotInterpolationSpeed = 150;

		public static float cam_topDown3D_minDistZ = 37;
		public static float cam_topDown3D_maxDistZ = 60;
		#endregion

		public static float cam_zoomSpeed = 2;
		public static int cam_maxZoomLevel = 8;

		public static int cam_type = (int)CameraType.TopDown;
		#endregion

		#region Tilted camera
		static void RegisterTiltedCameraCVars()
		{
			CVar.RegisterFloat("cam_tilted_posInterpolationSpeed", ref cam_tilted_posInterpolationSpeed);
			CVar.RegisterFloat("cam_tilted_rotInterpolationSpeed", ref cam_tilted_rotInterpolationSpeed);

			CVar.RegisterFloat("cam_tilted_minDistZ", ref cam_tilted_minDistZ);
			CVar.RegisterFloat("cam_tilted_maxDistZ", ref cam_tilted_maxDistZ);

			CVar.RegisterFloat("cam_tilted_distY", ref cam_tilted_distY);

			CVar.RegisterFloat("cam_tilted_minAngleX", ref cam_tilted_minAngleX);
			CVar.RegisterFloat("cam_tilted_maxAngleX", ref cam_tilted_maxAngleX);
		}

		public static float cam_tilted_posInterpolationSpeed = 5;
		public static float cam_tilted_rotInterpolationSpeed = 5;

		public static float cam_tilted_minDistZ = 37;
		public static float cam_tilted_maxDistZ = 60;

		public static float cam_tilted_distY = -17;

		public static float cam_tilted_minAngleX = -60;
		public static float cam_tilted_maxAngleX = -90;
		#endregion

		#region Tank movement
		static void RegisterTankMovementCVars()
		{
			CVar.RegisterFloat("tank_treadTurnMult", ref tank_treadTurnMult, "Wheen steering at full speed the slower tread is running at this percentage of full throttle");
			CVar.RegisterFloat("tank_maxTurnReductionSpeed", ref tank_maxTurnReductionSpeed, "Speed at which steering doesn't get any \"softer\"");
			CVar.RegisterInt("tank_debugMovement", ref tank_debugMovement);
		}
		public static float tank_treadTurnMult = 0.3f;
		public static float tank_maxTurnReductionSpeed = 4.0f;
		public static int tank_debugMovement = 0;
		#endregion

		#region Console commands
		static void RegisterConsoleCommands()
		{
			TurretTypes = (from type in Assembly.GetExecutingAssembly().GetTypes()
						   where type.Implements<TankTurret>()
						   select type).ToList();

			ConsoleCommand.Register("SetTurretType", (e) =>
			{
				if (e.Args.Length > 0)
					ForceTankType = e.Args[0];
			}, "Sets the turret type", CVarFlags.Cheat, true);

			ConsoleCommand.Register("ResetTurretType", (e) =>
			{
				ForceTankType = string.Empty;
			}, "Resets the forced turret type, set via SetTurretType.", CVarFlags.Cheat, true);

			ConsoleCommand.Register("SpawnAutoTurret", (e) =>
			{
				if (!Game.IsServer)
					return;

				var singlePlayer = GameRules.Current as SinglePlayer;

				var selector = new System.Random();

				var randomPlayer = singlePlayer.Players.ElementAt(selector.Next(singlePlayer.Players.Count()));
				if (randomPlayer != null)
				{
					var autoTurret = Entity.Spawn<AutoTurret>("pew", randomPlayer.Position + new Vec3(0, 0, 20));
				}
			}, "Spawns a auto turret above a random player", CVarFlags.None, true);

			ConsoleCommand.Register("DamageEntity", (e) =>
			{
				if (!Game.IsServer || e.Args.Length < 1)
					return;

				// First parameter has to be target name
				var damageableEntity = Entity.Find(e.Args[0]) as Entities.IDamageable;
				if (damageableEntity == null)
					return;

				var damage = damageableEntity.MaxHealth;
				// Second parameter is optional and specifies damage amount, otherwise fatal.
				if (e.Args.Length > 1)
					damage = int.Parse(e.Args[1]);

				damageableEntity.Damage(0, damage, Entities.DamageType.None, Vec3.Zero, Vec3.Zero);

			}, "Damages an entity", CVarFlags.None, true);
		}

		public static string ForceTankType;
		public static List<System.Type> TurretTypes;
		#endregion
	}
}
