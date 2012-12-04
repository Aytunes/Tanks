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

        protected override void PostSerialize()
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

                Input.ActionmapEvents.Add("moveright", OnRotateRight);
                Input.ActionmapEvents.Add("moveleft", OnRotateLeft);
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

            if (Network.IsMultiplayer)
                Entity.Spawn<Cursor>("Cursor", null, null, null, true, EntityFlags.CastShadow | EntityFlags.ClientOnly);
            else
                Entity.Spawn<Cursor>("Cursor");
		}

		protected override void OnEditorReset(bool enteringGame)
		{
			Reset(enteringGame);
		}

        [RemoteInvocation]
        void NetReset(bool enteringGame, string turretTypeName)
        {
            if (enteringGame)
            {
                var turretType = System.Type.GetType(turretTypeName);

                m_turret = System.Activator.CreateInstance(turretType, this) as TankTurret;
            }
            else
            {
                m_turret.Destroy();
                m_turret = null;
            }
        }

		private void Reset(bool enteringGame)
		{
            LoadObject("objects/tanks/tank_generic_" + Team + ".cdf");

            if(Network.IsServer)
            {
                string turretType;

                if (string.IsNullOrEmpty(ForceTankType))
                    turretType = TurretTypes[SinglePlayer.Selector.Next(TurretTypes.Count)].FullName;
                else
                    turretType = "CryGameCode.Tanks." + ForceTankType;

                if (IsLocalClient)
                    NetReset(enteringGame, turretType);

                Network.RemoteInvocation(NetReset, NetworkTarget.ToAllClients | NetworkTarget.NoLocalCalls, enteringGame, turretType);
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

            BoostTime = maxBoostTime;
            SpeedMultiplier = 1;
            BackwardsSpeedMultiplier = 0.85f;

			ReceiveUpdates = true;
		}

		public override void OnUpdate()
		{
			if(m_turret != null)
				m_turret.Update();

            if (BoostTime > 0 && IsBoosting)
                BoostTime -= Time.DeltaTime;
            else if (BoostTime < maxBoostTime && !IsBoosting)
                BoostTime += Time.DeltaTime;
		}

		string team;
		[EditorProperty]
		public string Team
		{
			get { return team ?? "red"; }
			set
			{
				var gameRules = GameRules.Current as SinglePlayer;
                if (gameRules != null && gameRules.IsTeamValid(value))
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
