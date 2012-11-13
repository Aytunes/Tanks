using CryEngine;
using CryGameCode.Entities;

namespace CryGameCode.Tanks
{
	public partial class Tank : DamageableActor
	{
        public override void OnSpawn()
        {
            Reset(true);
        }

		public void OnRevive()
		{
			ZoomLevel = 1;

            if (IsLocalClient)
            {
                Input.ActionmapEvents.Add("zoom_in", OnZoomIn);
                Input.ActionmapEvents.Add("zoom_out", OnZoomOut);

                Input.ActionmapEvents.Add("moveright", OnMoveRight);
                Input.ActionmapEvents.Add("moveleft", OnMoveLeft);
                Input.ActionmapEvents.Add("moveforward", OnMoveForward);
                Input.ActionmapEvents.Add("moveback", OnMoveBack);
                Input.ActionmapEvents.Add("sprint", OnSprint);
            }

			OnDestroyed += (e) =>
			{
                if(IsLocalClient)
				    Input.ActionmapEvents.RemoveAll(this);

				if(m_turret != null)
				{
					m_turret.Destroy();
					m_turret = null;
				}
			};

			Reset(true);

			Entity.Spawn<Cursor>("Cursor");
		}

		protected override void OnEditorReset(bool enteringGame)
		{
			Reset(enteringGame);
		}

		private void Reset(bool enteringGame)
		{
			LoadObject("objects/tanks/tank_generic_" + Team + ".cdf");

			if(enteringGame)
			{
				System.Type turretType;

				if(string.IsNullOrEmpty(ForceTankType))
					turretType = TurretTypes[SinglePlayer.Selector.Next(TurretTypes.Count)];
				else
					turretType = System.Type.GetType("CryGameCode.Tanks." + ForceTankType, true, true);

				m_turret = System.Activator.CreateInstance(turretType, this) as TankTurret;
			}
			else
			{
				m_turret.Destroy();
				m_turret = null;
			}

			m_leftTrack = GetAttachment("track_left");
			m_rightTrack = GetAttachment("track_right");

			// Unhide just in case
			Hide(false);

			Physics.AutoUpdate = false;
			Physics.Type = PhysicalizationType.Living;
			Physics.Mass = 500;
			Physics.HeightCollider = 1.2f;
			Physics.Slot = 0;
			Physics.UseCapsule = false;
			Physics.SizeCollider = new Vec3(2.2f, 2.2f, 0.2f);
			Physics.FlagsOR = PhysicalizationFlags.MonitorPostStep;
            Physics.MaxClimbAngle = MathHelpers.DegreesToRadians(30);
			Physics.AirControl = 0.0f;
			Physics.Save();

			InitHealth(100);

			ReceiveUpdates = true;
		}

		public override void OnUpdate()
		{
			if(m_turret != null)
				m_turret.Update();
		}

		string team;
		[EditorProperty]
		public string Team
		{
			get { return team ?? "red"; }
			set
			{
				var gameRules = GameRules.Current as SinglePlayer;
				if(gameRules != null && gameRules.IsTeamValid(value))
				{
					team = value;
				}
			}
		}

		private TankTurret m_turret;
		private Attachment m_leftTrack;
		private Attachment m_rightTrack;
	}
}
