using CryEngine;
using System;
using CryGameCode.Tanks;
using System.Reflection;

namespace CryGameCode.Entities.Garage
{
	[Entity(Category = "Garage")]
	public class TankModel : Entity
	{
		[EditorProperty]
		public string TurretType { get; set; }

		private Rigidbody m_turretInstance;

		protected override void  OnStartLevel()
		{
			Reset();
		}

		protected override void OnEditorReset(bool enteringGame)
		{
			Reset();
		}

		private void Reset()
		{
			if (m_turretInstance != null)
			{
				m_turretInstance.Remove();
				m_turretInstance = null;
			}

			LoadObject("objects/tanks/tank_generic_red.cdf");

			var physicalizationParams = new PhysicalizationParams(PhysicalizationType.Living);
			physicalizationParams.mass = 500;
			physicalizationParams.slot = 0;
			physicalizationParams.flagsOR = PhysicalizationFlags.MonitorPostStep;
			physicalizationParams.livingDimensions.heightCollider = 2.5f;
			physicalizationParams.livingDimensions.sizeCollider = new Vec3(2.2f, 2.2f, 1.2f);
			physicalizationParams.livingDimensions.heightPivot = 0;
			physicalizationParams.livingDynamics.kAirControl = 0;
			Physicalize(physicalizationParams);

			var requestedType = Type.GetType("CryGameCode.Tanks." + TurretType);

			if (requestedType == null)
			{
				Debug.LogWarning("Invalid turret name: {0}", TurretType);
				return;
			}

			var turretInfo = Activator.CreateInstance(requestedType, (Tank)null) as TankTurret;

			m_turretInstance = Entity.Spawn<Rigidbody>("displayTurret");
			m_turretInstance.Model = turretInfo.Model;
			m_turretInstance.Position = Position + Rotation * Tank.TurretOffset;
			m_turretInstance.Rotation = Rotation;
		}
	}
}
