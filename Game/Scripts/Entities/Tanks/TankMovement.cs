using System;

using CryEngine;

using CryGameCode.Network;

namespace CryGameCode.Tanks
{
	public partial class Tank
	{
		#region NetConstants

		const float MaxDelta = 2;
		const float MinDelta = 0.2f;
		const float DeltaMult = 2f;

		#endregion

		#region MovementConstants

		//TODO: remove magic numbers
		const float TankMass = 1000;
		const float TankFrontalArea = 20.6f;
		const float TankDragCoefficient = 0.8f;
		const float AirDensity = 1.27f;
		const float momentumIntertia = 4000.0f; // kg*m²

		#endregion

		private float m_lastAngularAcceleration;
		private float m_lastAngularAccelerationChanged;

		float NormalToAngle(Vec3 normal)
		{
			return (float)Math.Atan2(Math.Sqrt(normal.X * normal.X + normal.Y * normal.Y), normal.Z);
		}

		protected void UpdateMovement()
		{
			if (IsDestroyed || IsDead)
				return;

			if (m_treads[0] == null || m_treads[1] == null)
			{
				return;
			}

			var frameTime = Time.DeltaTime;

			// update desired movement changes based on input.
			UpdateTreads(frameTime, Velocity);

			var moveRequest = new EntityMovementRequest();
			moveRequest.type = EntityMoveType.Normal;

			///////////////////////////
			// Common
			///////////////////////////

			Vec3 prevVelocity = Velocity;
			var prevRotation = Rotation;

			var forwardDir = prevRotation.Column1;

			Vec3 normalizedVelocity;
			if (!prevVelocity.IsZero())
				normalizedVelocity = prevVelocity.Normalized;
			else
				normalizedVelocity = forwardDir.Normalized;

			var livingStatus = Physics.LivingStatus;

			var groundFriction = livingStatus.GroundSurfaceType.Parameters.Friction;
			if (groundFriction == 0)
				groundFriction = 1;

			var onGround = !livingStatus.IsFlying;

			var slopeAngle = onGround ? NormalToAngle(GroundNormal) : MathHelpers.DegreesToRadians(90);

			var totalForce = m_treads[0].Force + m_treads[1].Force;

			///////////////////////////
			// Set physics parameters
			///////////////////////////
			var simulationParams = PhysicalSimulationParameters.Create();

			if (AlignGravityToUpVector)
				simulationParams.gravity = prevRotation.Column2.Normalized * CVar.Get("p_gravity_z").FVal;
			else
				simulationParams.gravity = new Vec3(0, 0, CVar.Get("p_gravity_z").FVal);

			Physics.SetSimulationParameters(ref simulationParams);

			///////////////////////////
			// Rotation
			///////////////////////////
			var totalMomentum = m_treads[0].Force * m_treads[0].LocalPos.X + m_treads[1].Force * m_treads[1].LocalPos.X;
			// M = I * a
			var angularAcceleration = totalMomentum / momentumIntertia;

			if (angularAcceleration != m_lastAngularAcceleration)
			{
				m_lastAngularAccelerationChanged = Time.FrameStartTime;
				m_lastAngularAcceleration = angularAcceleration;
			}

			var right = prevRotation.Column0;
			var forward = (GroundNormal % right).Normalized;

			moveRequest.rotation = prevRotation.Inverted * new Quat(Matrix33.CreateFromVectors(forward % GroundNormal, forward, GroundNormal));
			moveRequest.rotation.Normalize();

			moveRequest.rotation *= Quat.CreateRotationZ(angularAcceleration * frameTime);

			//Only start interpolating shortly after acceleration changes have been made
			if (Game.IsPureClient && (Time.FrameStartTime - m_lastAngularAccelerationChanged) >= 200.0f)
			{
				//Renderer.DrawTextToScreen(100, 80, 1.3f, Color.White, "Lerping! {0}", Time.FrameStartTime - m_lastAngularAccelerationChanged);
				Rotation = Quat.CreateNlerp(Rotation, m_serverRot, Time.DeltaTime);
			}

			///////////////////////////
			// Velocity
			///////////////////////////

			float mass = TankMass;
			float frontalArea = TankFrontalArea;

			float dragCoefficient = TankDragCoefficient;

			if (Turret != null)
			{
				mass += Turret.Mass;
				frontalArea += Turret.FrontalArea;
				dragCoefficient += Turret.DragCoefficient;
			}

			var terminalVelocity = (float)Math.Sqrt(Math.Abs(2 * mass * Math.Abs(CVar.Get("p_gravity_z").FVal) * (Math.Sin(slopeAngle) - groundFriction * Math.Cos(slopeAngle))) / (AirDensity * dragCoefficient * frontalArea));
			var velocityRatio = prevVelocity.Length / terminalVelocity;

			//F = m*a
			var acceleration = (totalForce / mass);
			var forwardAcceleration = forwardDir * acceleration;// *GameCVars.tank_movementSpeedMult;

			//TODO: Do proper tread-dependant friction and calculation
			var frictionDeceleration = (normalizedVelocity * velocityRatio) * (float)(groundFriction * Math.Abs(CVar.Get("p_gravity_z").FVal) * Math.Cos(slopeAngle));

			var dragDeceleration = (dragCoefficient * frontalArea * AirDensity * (normalizedVelocity * (float)Math.Pow(prevVelocity.Length, 2))) / (2 * mass);

			moveRequest.velocity = prevVelocity + (forwardAcceleration - frictionDeceleration - dragDeceleration);

			if (Game.IsPureClient && m_currentDelta.Length > MinDelta)
				moveRequest.velocity += m_currentDelta * DeltaMult * m_currentDelta.LengthSquared;

			if (GameCVars.tank_debugMovement != 0)
			{
				Renderer.DrawTextToScreen(100, 80, 1.3f, Color.White, "TerminalVel: {0}", terminalVelocity);
				Renderer.DrawTextToScreen(100, 90, 1.3f, Color.White, "Speed: {0}", moveRequest.velocity.Length);
				Renderer.DrawTextToScreen(100, 100, 1.3f, Color.Red, "angularAcceleration: {0}", angularAcceleration);
				Renderer.DrawTextToScreen(100, 120, 1.2f, Color.White, "acceleration: {0}", forwardAcceleration - frictionDeceleration - dragDeceleration);
				Renderer.DrawTextToScreen(100, 130, 1.2f, Color.Red, "forceLeft: {0}", Math.Floor(m_treads[1].Force));
				Renderer.DrawTextToScreen(100, 140, 1.2f, Color.Red, "forceRight: {0}", Math.Floor(m_treads[0].Force));
				Renderer.DrawTextToScreen(100, 150, 1.3f, Color.Green, "totalMomentum: {0}", Math.Floor(totalMomentum));
				Renderer.DrawTextToScreen(100, 160, 1.3f, Color.Blue, "lThrottle: {0} rThrottle: {1}", m_treads[1].GetThrottle(), m_treads[0].GetThrottle());
				Renderer.DrawTextToScreen(100, 170, 1.3f, Color.Green, "slopeAngle: {0}", slopeAngle);

			}

			AddMovement(ref moveRequest);

			if (Game.IsServer)
				GameObject.NotifyNetworkStateChange((int)NetAspects.Movement);
		}

		private Material GetTrackMaterial(float moveDirection)
		{
			if (Math.Abs(moveDirection) <= 0.05f)
				return Material.Find("objects/tanks/tracks");
			else if (moveDirection < 0)
				return Material.Find("objects/tanks/tracksmoving_back");

			return Material.Find("objects/tanks/tracksmoving_forward");
		}

		void UpdateTreads(float frameTime, Vec3 velocity)
		{
			if (Input == null)
				return;

			m_treads[0].Update();
			m_treads[1].Update();

			var maxTurnReductionSpeed = GameCVars.tank_maxTurnReductionSpeed;
			var turnMult = GameCVars.tank_treadTurnMult;
			var speed = velocity.Length;

			var hardcore = false;
			if (hardcore)
			{
				m_treads[0].SetThrottle(0.0f);
				m_treads[1].SetThrottle(0.0f);

				if (Input.HasFlag(InputFlags.MoveForward))
				{
					m_treads[1].SetThrottle(1.0f);
				}
				if (Input.HasFlag(InputFlags.MoveLeft))
				{
					m_treads[1].SetThrottle(-1.0f);
				}
				if (Input.HasFlag(InputFlags.MoveBack))
				{
					m_treads[0].SetThrottle(1.0f);
				}
				if (Input.HasFlag(InputFlags.MoveRight))
				{
					m_treads[0].SetThrottle(-1.0f);
				}
			}
			else
			{
				if (Input.HasFlag(InputFlags.MoveForward))
				{

					m_treads[0].SetThrottle(1.0f);
					m_treads[1].SetThrottle(1.0f);

					if (Input.HasFlag(InputFlags.MoveLeft))
					{
						m_treads[0].SetThrottle(1.0f);
						m_treads[1].SetThrottle(MathHelpers.Clamp(turnMult * (speed / maxTurnReductionSpeed), 0, turnMult));
					}
					if (Input.HasFlag(InputFlags.MoveRight))
					{
						m_treads[1].SetThrottle(1.0f);
						m_treads[0].SetThrottle(MathHelpers.Clamp(turnMult * (speed / maxTurnReductionSpeed), 0, turnMult));
					}
				}
				else if (Input.HasFlag(InputFlags.MoveBack))
				{
					m_treads[0].SetThrottle(-1.0f);
					m_treads[1].SetThrottle(-1.0f);

					if (Input.HasFlag(InputFlags.MoveLeft))
					{
						m_treads[1].SetThrottle(-1.0f);
						m_treads[0].SetThrottle(MathHelpers.Clamp(-turnMult * (speed / maxTurnReductionSpeed), -turnMult, 0));
					}
					if (Input.HasFlag(InputFlags.MoveRight))
					{
						m_treads[0].SetThrottle(-1.0f);
						m_treads[1].SetThrottle(MathHelpers.Clamp(-turnMult * (speed / maxTurnReductionSpeed), -turnMult, 0));
					}
				}
				else if (Input.HasFlag(InputFlags.MoveLeft))
				{
					m_treads[0].SetThrottle(1.0f);
					m_treads[1].SetThrottle(-1.0f);
				}
				else if (Input.HasFlag(InputFlags.MoveRight))
				{
					m_treads[1].SetThrottle(1.0f);
					m_treads[0].SetThrottle(-1.0f);
				}
				else
				{
					m_treads[0].SetThrottle(0.0f);
					m_treads[1].SetThrottle(0.0f);
				}
			}
		}
	}
}