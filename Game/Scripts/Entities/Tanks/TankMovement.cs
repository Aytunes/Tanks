using System;

using CryEngine;

namespace CryGameCode.Tanks
{
	public partial class Tank
	{
		const int MovementAspect = 128;
		const float MaxDelta = 2;
		const float MinDelta = 0.2f;
		const float DeltaMult = 2f;

		float NormalToAngle(Vec3 normal)
		{
			return (float)Math.Atan2(Math.Sqrt(normal.X * normal.X + normal.Y * normal.Y), normal.Z);
		}

		protected void UpdateMovement()
		{
			if (IsDestroyed || IsDead)
				return;

            if (m_threads[0] == null || m_threads[1] == null)
            {
                return;
            }

			var frameTime = Time.DeltaTime;

			// update desired movement changes based on input.
			UpdateThreads(frameTime, Velocity);

			var moveRequest = new EntityMovementRequest();
			moveRequest.type = EntityMoveType.Normal;

			///////////////////////////
			// Common
			///////////////////////////

			Vec3 prevVelocity = Velocity;
			var prevRotation = Rotation;
			var normalizedRotation = prevRotation.Normalized;

			var forwardDir = prevRotation.Column1;
			var upDir = prevRotation.Column2;

			Vec3 normalizedVelocity;
			if (!prevVelocity.IsZero())
				normalizedVelocity = prevVelocity.Normalized;
			else
				normalizedVelocity = forwardDir.Normalized;

			var groundFriction = Physics.Status.Living.GroundSurfaceType.Parameters.Friction;

			var onGround = !Physics.Status.Living.IsFlying;

			var slopeAngle = onGround ? NormalToAngle(GroundNormal) : MathHelpers.DegreesToRadians(90);

            var totalForce = m_threads[0].Force + m_threads[1].Force;

            ///////////////////////////
            // Rotation
            ///////////////////////////
            var totalMomentum = m_threads[0].Force * m_threads[0].LocalPos.X + m_threads[1].Force * m_threads[1].LocalPos.X;
            var momentumIntertia = 500.0f * 200.0f; // kg*m²
            // M = I * a
            var angularAcceleration =  totalMomentum / momentumIntertia;

            var turnRot = Quat.CreateRotationZ((angularAcceleration * frameTime));
            moveRequest.rotation = turnRot;
            moveRequest.rotation.Normalize();


			///////////////////////////
			// Velocity
			///////////////////////////
            //TODO: remove magic numbers
			const float tankMass = 500;
			const float tankFrontalArea = 20.6f;
			const float tankDragCoefficient = 0.9f;
            const float airDensity = 1.27f;

			float mass = tankMass + Turret.Mass;
			float frontalArea = tankFrontalArea + Turret.FrontalArea;
			float dragCoefficient = tankDragCoefficient + Turret.DragCoefficient;

			var terminalVelocity = (float)Math.Sqrt(Math.Abs(2 * mass * Math.Abs(CVar.Get("p_gravity_z").FVal) * (Math.Sin(slopeAngle) - groundFriction * Math.Cos(slopeAngle))) / (airDensity * dragCoefficient * frontalArea));
			var velocityRatio = prevVelocity.Length / terminalVelocity;

            //F = m*a
            var acceleration = (totalForce / mass) * frameTime;
            var forwardAcceleration = forwardDir * acceleration;// *GameCVars.tank_movementSpeedMult;

            //TODO: Do proper thread-dependant friction and calculation
			var frictionDeceleration = (normalizedVelocity * velocityRatio) * (float)(groundFriction * Math.Abs(CVar.Get("p_gravity_z").FVal) * Math.Cos(slopeAngle));

            //Is this even relevant for a massive tank driving at 12 km/h?
			var dragDeceleration = (dragCoefficient * frontalArea * airDensity * (normalizedVelocity * (float)Math.Pow(prevVelocity.Length, 2))) / (4 * mass);

			moveRequest.velocity = prevVelocity + (forwardAcceleration - frictionDeceleration - dragDeceleration);

			if (Game.IsPureClient && m_currentDelta.Length > MinDelta)
				moveRequest.velocity += m_currentDelta * DeltaMult * m_currentDelta.LengthSquared;


            if (m_debug)
            {
                Renderer.DrawTextToScreen(100, 80, 1.3f, Color.White, "TerminalVel: {0}", terminalVelocity);
                Renderer.DrawTextToScreen(100, 90, 1.3f, Color.White, "Speed: {0}", moveRequest.velocity.Length);
                Renderer.DrawTextToScreen(100, 100, 1.3f, Color.White, "angularVel: {0}", angularAcceleration);
                Renderer.DrawTextToScreen(100, 110, 1.2f, Color.White, "angleChange: {0}", turnRot.Column1);
                Renderer.DrawTextToScreen(100, 120, 1.2f, Color.White, "acceleration: {0}", forwardAcceleration - frictionDeceleration - dragDeceleration);
                Renderer.DrawTextToScreen(100, 130, 1.2f, Color.Red, "forceLeft: {0}", Math.Floor(m_threads[1].Force));
                Renderer.DrawTextToScreen(100, 140, 1.2f, Color.Red, "forceRight: {0}", Math.Floor(m_threads[0].Force));
                Renderer.DrawTextToScreen(100, 150, 1.3f, Color.Green, "totalMomentum: {0}", Math.Floor(totalMomentum));
            }

			AddMovement(ref moveRequest);

			if (Game.IsServer)
				GameObject.NotifyNetworkStateChange(MovementAspect);
		}

		private Material GetTrackMaterial(float moveDirection)
		{
			if (Math.Abs(moveDirection) <= 0.05f)
				return Material.Find("objects/tanks/tracks");
			else if (moveDirection < 0)
				return Material.Find("objects/tanks/tracksmoving_back");

			return Material.Find("objects/tanks/tracksmoving_forward");
		}

		void UpdateThreads(float frameTime, Vec3 velocity)
		{
			if (Input == null)
				return;

            var maxForce = GameCVars.tank_threadMaxForce;
            var maxTurnReductionSpeed = GameCVars.tank_maxTurnReductionSpeed;
            var turnMult = GameCVars.tank_threadTurnMult;
            var speed = velocity.Length;
            
            m_threads[0].Force = 0.0f;
            m_threads[1].Force = 0.0f;

            if (Input.HasFlag(InputFlags.MoveLeft))
			{
                m_threads[0].Force = 1.0f;
                m_threads[1].Force = -1.0f;
			}
            else if (Input.HasFlag(InputFlags.MoveRight))
			{
                m_threads[1].Force = 1.0f;
                m_threads[0].Force = -1.0f;
			}
            if (Input.HasFlag(InputFlags.MoveForward))
			{
                m_threads[0].Force = 1.0f;
                m_threads[1].Force = 1.0f;

                if (Input.HasFlag(InputFlags.MoveLeft))
                {
                    m_threads[0].Force = 1.0f;
                    m_threads[1].Force = MathHelpers.Clamp(turnMult * (speed / maxTurnReductionSpeed), 0, turnMult);
                }
                if (Input.HasFlag(InputFlags.MoveRight))
                {
                    m_threads[1].Force = 1.0f;
                    m_threads[0].Force = MathHelpers.Clamp(turnMult * (speed / maxTurnReductionSpeed), 0, turnMult);
                }
			}
            else if (Input.HasFlag(InputFlags.MoveBack))
			{
                m_threads[0].Force = -1.0f;
                m_threads[1].Force = -1.0f;

                if (Input.HasFlag(InputFlags.MoveLeft))
                {
                    m_threads[0].Force = -1.0f;
                    m_threads[1].Force = MathHelpers.Clamp(-turnMult * (speed / maxTurnReductionSpeed), -turnMult, 0); ;
                }
                if (Input.HasFlag(InputFlags.MoveRight))
                {
                    m_threads[1].Force = -1.0f;
                    m_threads[0].Force = MathHelpers.Clamp(-turnMult * (speed / maxTurnReductionSpeed), -turnMult, 0); ;
                }
			}

            m_threads[0].Force *= maxForce;
            m_threads[1].Force *= maxForce;
		}

        bool m_debug = true;
	}
}