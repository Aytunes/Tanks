using System;

using CryEngine;
using CryEngine.Extensions;

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

			var frameTime = Time.DeltaTime;

			// update desired m_acceleration based on input.
			UpdateAcceleration(frameTime);

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
				normalizedVelocity = forwardDir;

			var groundFriction = Physics.Status.Living.GroundSurfaceType.Parameters.Friction;

			var onGround = !Physics.Status.Living.IsFlying;

			var slopeAngle = onGround ? NormalToAngle(GroundNormal) : MathHelpers.DegreesToRadians(90);

			///////////////////////////
			// Velocity
			///////////////////////////
			const float tankMass = 500;
			const float tankFrontalArea = 20.6f;
			const float tankDragCoefficient = 0.9f;

			float mass = tankMass + Turret.Mass;
			float frontalArea = tankFrontalArea + Turret.FrontalArea;
			float dragCoefficient = tankDragCoefficient + Turret.DragCoefficient;

			const float airDensity = 1.27f;

			var terminalVelocity = (float)Math.Sqrt(Math.Abs(2 * mass * Math.Abs(CVar.Get("p_gravity_z").FVal) * (Math.Sin(slopeAngle) - groundFriction * Math.Cos(slopeAngle))) / (airDensity * dragCoefficient * frontalArea));
			var velocityRatio = prevVelocity.Length / terminalVelocity;

			var acceleration = m_acceleration.X + m_acceleration.Y;
			var forwardAcceleration = forwardDir * acceleration * GameCVars.tank_movementSpeedMult;

			var frictionDeceleration = (normalizedVelocity * velocityRatio) * (float)(groundFriction * Math.Abs(CVar.Get("p_gravity_z").FVal) * Math.Cos(slopeAngle));

			var dragDeceleration = (dragCoefficient * frontalArea * airDensity * (normalizedVelocity * (float)Math.Pow(prevVelocity.Length, 2))) / mass;

			moveRequest.velocity = prevVelocity + (forwardAcceleration - frictionDeceleration - dragDeceleration);

			if (Game.IsPureClient && m_currentDelta.Length > MinDelta)
				moveRequest.velocity += m_currentDelta * DeltaMult * m_currentDelta.LengthSquared;

			///////////////////////////
			// Rotation
			///////////////////////////

			// turning
			float angleChange = ((m_acceleration.X - m_acceleration.Y) / 2) * Time.DeltaTime * GameCVars.tank_rotationSpeed;

			var turnRot = Quat.CreateRotationZ(angleChange);

			moveRequest.rotation = turnRot;
			moveRequest.rotation.Normalize();

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

		void UpdateAcceleration(float frameTime)
		{
			if (Input == null)
				return;

			var accelerationSpeed = GameCVars.tank_accelerationSpeed * frameTime;
			var accelerationSpeedRotation = GameCVars.tank_accelerationSpeedRotation * frameTime;

			var maxAcceleration = GameCVars.tank_maxAcceleration;
            if (Input.Flags.ContainsFlag(InputFlags.Boost))
				maxAcceleration = GameCVars.tank_maxAccelerationBoosting;

			// in order to make the tank feel heavy, hinder forward / backwards movement when attempting to turn.
            if (Input.Flags.ContainsFlag(InputFlags.MoveLeft))
			{
				m_acceleration.X += accelerationSpeedRotation;
				m_acceleration.Y -= accelerationSpeedRotation;
			}
            else if (Input.Flags.ContainsFlag(InputFlags.MoveRight))
			{
				m_acceleration.X -= accelerationSpeedRotation;
				m_acceleration.Y += accelerationSpeedRotation;
			}
            else if (Input.Flags.ContainsFlag(InputFlags.MoveForward))
			{
				m_acceleration.X += accelerationSpeed;
				m_acceleration.Y += accelerationSpeed;
			}
            else if (Input.Flags.ContainsFlag(InputFlags.MoveBack))
			{
				m_acceleration.X -= accelerationSpeed;
				m_acceleration.Y -= accelerationSpeed;
			}
			else
			{
				MathHelpers.Interpolate(ref m_acceleration.X, 0, GameCVars.tank_decelerationSpeed);
				MathHelpers.Interpolate(ref m_acceleration.Y, 0, GameCVars.tank_decelerationSpeed);
			}

			m_acceleration.X = MathHelpers.Clamp(m_acceleration.X, -maxAcceleration, maxAcceleration);
			m_acceleration.Y = MathHelpers.Clamp(m_acceleration.Y, -maxAcceleration, maxAcceleration);
		}

		Vec2 m_acceleration = new Vec2();
	}
}