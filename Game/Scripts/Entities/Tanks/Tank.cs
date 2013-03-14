using System;
using System.Linq;
using CryEngine;
using CryEngine.Serialization;
using CryGameCode.Entities;

namespace CryGameCode.Tanks
{
	public partial class Tank : DamageableActor
	{
		public Tank()
		{
			Debug.LogAlways("[Enter] Tank.ctor: actor {0}", Id);

			MaxHealth = 100;

			m_acceleration = new Vec2();

			Input = new PlayerInput(this);
		}

		/// <summary>
		/// Called when the client has finished loading and is ready to play.
		/// </summary>
		public void OnEnteredGame()
		{
			if (IsLocalClient)
				Input.RegisterInputs();

			Debug.LogAlways("[Enter] Tank.OnEnteredGame: actor {0}", Id);

			PrePhysicsUpdateMode = PrePhysicsUpdateMode.Always;
			ReceivePostUpdates = true;

			Flags |= EntityFlags.CastShadow;
			ZoomLevel = 1;
			Health = 0;
			Hide(true);
			ReceiveUpdates = true;
		}

		public void OnLeftGame()
		{
			if (Input != null)
				Input.Destroy();

			if (Turret != null)
			{
				Turret.Destroy();
				Turret = null;
			}
		}

		protected override void NetSerialize(CrySerialize serialize, int aspect, byte profile, int flags)
		{
			serialize.BeginGroup("TankActor");

			// input aspect
			if (aspect == PlayerInput.Aspect)
			{
				if (Input != null)
					Input.NetSerialize(serialize);
				else
					serialize.FlagPartialRead();
			}

			if (aspect == MovementAspect)
			{
				if (Game.IsServer)
				{
					m_serverPos = Position;
					m_serverRot = Rotation;
				}
				
				serialize.Value("pos", ref m_serverPos, "wrld");
				serialize.Value("rot", ref m_serverRot);
			}

			serialize.EndGroup();
		}

		void ResetModel()
		{
			LoadObject("objects/tanks/tank_generic_" + Team + ".cdf");

			Physicalize();
		}

		public void OnRevived()
		{
			Turret = Activator.CreateInstance(Type.GetType(TurretTypeName), this) as TankTurret;

			Health = MaxHealth;

			ResetModel();

			Hide(false);

			if (IsLocalClient)
				Entity.Spawn<Cursor>("Cursor");
		}

		protected override void OnEditorReset(bool enteringGame)
		{
			Health = 0;

			if (enteringGame)
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

			Physics.Gravity = Vec3.Zero;
			Physics.AirControl = 0;
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

			if (Input != null)
				Input.Update();

			Turret.Update();

			if (Physics.Status != null)
			{
				float blend = MathHelpers.Clamp(Time.DeltaTime / 0.15f, 0, 1.0f);
				GroundNormal = (GroundNormal + blend * (Physics.Status.Living.GroundNormal - GroundNormal));
			}

			if (Game.IsPureClient)
			{
				var currentPos = Position;
				var currentRot = Rotation;

				m_currentDelta = m_serverPos - currentPos;

				if (IsLocalClient)
				{
					Renderer.DrawTextToScreen(10, 10, 2, Color.White, "Client pos: {0}", currentPos);
					Renderer.DrawTextToScreen(10, 30, 2, Color.White, "Server pos: {0}", m_serverPos);
					Renderer.DrawTextToScreen(10, 50, 2, Color.White, "Delta: {0}", m_currentDelta.Length);
				}

				// Start forcing sync if we have to
				// TODO: Tweak based on connection
				if (m_currentDelta.Length > MaxDelta)
				{
					Position = m_serverPos;
				}

				Rotation = Quat.CreateNlerp(currentRot, m_serverRot, Time.DeltaTime * 20);
			}

			if (Turret != null && Turret.TurretEntity != null)
				Turret.TurretEntity.Position = Position + Rotation * new Vec3(0, 0.69252968f, 2.05108f);
		}

		protected override void OnPrePhysicsUpdate()
		{
			if (Input != null)
				Input.PreUpdate();

			UpdateMovement();
		}

		public void ToggleSpectatorPoint(bool increment = false)
		{
			if (!IsDead)
				return;

			var spectatorPoints = Entity.GetByClass<SpectatorPoint>();
			var spectatorPointCount = spectatorPoints.Count();

			if (spectatorPointCount > 0)
			{
				if (increment)
					CurrentSpectatorPoint++;

				if (CurrentSpectatorPoint >= spectatorPointCount)
					CurrentSpectatorPoint = 0;

				var iSpectatorPoint = SinglePlayer.Selector.Next(CurrentSpectatorPoint, spectatorPointCount);
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

		private Vec3 m_currentDelta;
		private Vec3 m_serverPos;
		private Quat m_serverRot;

		public string TurretTypeName { get; set; }

		public PlayerInput Input { get; set; }

		public TankTurret Turret { get; set; }

		public Vec3 GroundNormal { get; set; }

		public int CurrentSpectatorPoint { get; set; }
	}
}
