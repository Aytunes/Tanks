using CryEngine;
using CryGameCode.Entities;

using System;
using System.Linq;

namespace CryGameCode.Tanks
{
	public partial class Tank : DamageableActor
	{
        public Tank()
        {
            MaxHealth = 100;
        }

        /// <summary>
        /// Called when the client has finished loading and is ready to play.
        /// </summary>
        public void OnEnteredGame()
        {
            m_tankInput = new PlayerInput(this);

            ZoomLevel = 1;

            OnDestroyed += (e) =>
            {
                m_tankInput.Destroy();

                if (Turret != null)
                {
                    Turret.Destroy();
                    Turret = null;
                }

                ReceiveUpdates = false;
            };

            var gameMode = GameRules.Current as SinglePlayer;

            // TODO: Allow picking of Team + Turret via UI
            Team = gameMode.Teams.ElementAt(SinglePlayer.Selector.Next(0, gameMode.Teams.Length - 1));

            LoadObject("objects/tanks/tank_generic_" + Team + ".cdf");

            Type turretType;

            if (string.IsNullOrEmpty(GameCVars.ForceTankType))
                turretType = GameCVars.TurretTypes[SinglePlayer.Selector.Next(GameCVars.TurretTypes.Count)];
            else
            {
                turretType = Type.GetType("CryGameCode.Tanks." + GameCVars.ForceTankType);
                if (turretType == null)
                {
                    turretType = typeof(Autocannon);
                    Debug.LogAlways("Forced turret type {0} could not be located", GameCVars.ForceTankType);
                }
            }

            Turret = System.Activator.CreateInstance(turretType, this) as TankTurret;

            m_leftTrack = GetAttachment("track_left");
            m_rightTrack = GetAttachment("track_right");

            Health = 0;

            Hide(true);

            Reset(true);
        }

        public void OnRevived()
        {
            Health = MaxHealth;

            Turret.Reset();

            Hide(false);
        }

		protected override void OnEditorReset(bool enteringGame)
		{
            Health = 0;

            if(enteringGame)
                ToggleSpectatorPoint();
            
			Reset(enteringGame);
		}

		private void Reset(bool enteringGame)
		{
			/*Physics.AutoUpdate = false;
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

            m_acceleration = new Vec2();*/

			ReceiveUpdates = true;
		}

		public override void OnUpdate()
		{
			if(Turret != null)
				Turret.Update();

            if (Physics.Status != null)
            {
                float blend = MathHelpers.Clamp(Time.DeltaTime / 0.15f, 0, 1.0f);
                GroundNormal = (GroundNormal + blend * (Physics.Status.Living.GroundNormal - GroundNormal));
            }
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

        public void ToggleSpectatorPoint(bool increment = false)
        {
            if (!IsDead)
                return;

            var spectatorPoints = Entity.GetByClass<SpectatorPoint>();
            var spectatorPointCount = spectatorPoints.Count();

            if (spectatorPointCount > 0)
            {
                if(increment)
                    CurrentSpectatorPoint++;

                if (CurrentSpectatorPoint >= spectatorPointCount)
                    CurrentSpectatorPoint = 0;

                var iSpectatorPoint = SinglePlayer.Selector.Next(CurrentSpectatorPoint, spectatorPointCount - 1);
                var spectatorPoint = spectatorPoints.ElementAt(iSpectatorPoint);

                Position = spectatorPoint.Position;
                Rotation = spectatorPoint.Rotation;
            }

            Hide(true);
        }

        private PlayerInput m_tankInput;

        public TankTurret Turret { get; set; }

		private Attachment m_leftTrack;
		private Attachment m_rightTrack;

        public Vec3 GroundNormal { get; set; }

        public int CurrentSpectatorPoint { get; set; }
	}
}
