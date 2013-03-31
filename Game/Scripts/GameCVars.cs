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
