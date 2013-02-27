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

            m_acceleration = new Vec2();
        }

        /// <summary>
        /// Called when the client has finished loading and is ready to play.
        /// </summary>
        public void OnEnteredGame()
        {
            var gameMode = GameRules.Current as SinglePlayer;

            m_tankInput = new PlayerInput(this);

            if (IsLocalClient)
            {
                // Set team & turret type, sent to server and remote clients on revival. (TODO: Allow picking via UI)
                Team = gameMode.Teams.ElementAt(SinglePlayer.Selector.Next(0, gameMode.Teams.Length - 1));

                if (string.IsNullOrEmpty(GameCVars.ForceTankType))
                    TurretTypeName = GameCVars.TurretTypes[SinglePlayer.Selector.Next(GameCVars.TurretTypes.Count)].FullName;
                else
                    TurretTypeName = "CryGameCode.Tanks." + GameCVars.ForceTankType;
            }

            ZoomLevel = 1;

            OnDestroyed += (e) =>
            {
                if(m_tankInput != null)
                    m_tankInput.Destroy();

                if (Turret != null)
                {
                    Turret.Destroy();
                    Turret = null;
                }
            };

            Health = 0;

            Hide(true);

            ReceiveUpdates = true;
        }

        protected override void NetSerialize(CryEngine.Serialization.CrySerialize serialize, int aspect, byte profile, int flags)
        {
            serialize.BeginGroup("TankActor");

            if(m_tankInput != null)
                m_tankInput.NetSerialize(serialize);

            serialize.EndGroup();
        }

        void ResetModel()
        {
            LoadObject("objects/tanks/tank_generic_" + Team + ".cdf");

            m_leftTrack = GetAttachment("track_left");
            m_rightTrack = GetAttachment("track_right");

            Physicalize();
        }

        public void OnRevived()
        {
            Health = MaxHealth;

            ResetModel();

            Turret = Activator.CreateInstance(Type.GetType(TurretTypeName), this) as TankTurret;

            Hide(false);

            if (IsLocalClient)
                Entity.Spawn<Cursor>("Cursor");
        }

		protected override void OnEditorReset(bool enteringGame)
		{
            Health = 0;

            if(enteringGame)
                ToggleSpectatorPoint();
		}

        void Physicalize()
        {
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
        }

		/*private void Reset(bool enteringGame)
		{
		    Physicalize();	

            m_acceleration = new Vec2();
		}*/

		public override void OnUpdate()
		{
            if (IsDead)
                return;

			Turret.Update();

            if (Physics.Status != null)
            {
                float blend = MathHelpers.Clamp(Time.DeltaTime / 0.15f, 0, 1.0f);
                GroundNormal = (GroundNormal + blend * (Physics.Status.Living.GroundNormal - GroundNormal));
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

        string team;
		public string Team
		{
			get { return team ?? "red"; }
			set
			{
				var gameRules = GameRules.Current as SinglePlayer;
                if (gameRules.IsTeamValid(value))
                {
                    team = value;

                    // Load correct model for this team
                    ResetModel();
                }
			}
		}

        public string TurretTypeName { get; set; }

        private PlayerInput m_tankInput;

        public TankTurret Turret { get; set; }

		private Attachment m_leftTrack;
		private Attachment m_rightTrack;

        public Vec3 GroundNormal { get; set; }

        public int CurrentSpectatorPoint { get; set; }
	}
}
