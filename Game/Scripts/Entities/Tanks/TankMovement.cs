using System;
using CryEngine;

namespace CryGameCode.Tanks
{
	public partial class Tank
	{
        float NormalToAngle(Vec3 normal)
        {
            return (float)Math.Atan2(Math.Sqrt(normal.X * normal.X + normal.Y * normal.Y), normal.Z);
        }

		protected override void OnPrePhysicsUpdate()
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

            // Used to determine whether the side of the tank is facing the current velocity direction.
            var travelDirectionDot = normalizedVelocity.Dot(forwardDir);

            var groundFriction = Physics.Status.Living.GroundSurfaceType.Parameters.Friction;

            var onGround = !Physics.Status.Living.IsFlying;

            var slopeAngle = onGround ? NormalToAngle(GroundNormal) : MathHelpers.DegreesToRadians(90);

            ///////////////////////////
            // Velocity
            ///////////////////////////

            var acceleration = m_acceleration.X + m_acceleration.Y;
            var forwardAcceleration = forwardDir * acceleration * GameCVars.tank_movementSpeedMult;

            var frictionDeceleration = normalizedVelocity * (float)(groundFriction * Math.Abs(CVar.Get("p_gravity_z").FVal) * Math.Cos(slopeAngle));

            var dragDeceleration = (1.2f * 0.588f * 1.27f * (normalizedVelocity * (float)Math.Pow(prevVelocity.Length, 2))) / Physics.Status.Dynamics.Mass;

            moveRequest.velocity = prevVelocity + (forwardAcceleration - frictionDeceleration - dragDeceleration) * Time.DeltaTime;
            moveRequest.velocity.ClampLength(GameCVars.tank_movementMaxSpeed);

            ///////////////////////////
            // Rotation
            ///////////////////////////

            // turning
            float angleChange = ((m_acceleration.X - m_acceleration.Y) / 2) * Time.DeltaTime * GameCVars.tank_rotationSpeed;

            var turnRot = Quat.CreateRotationZ(angleChange);

            moveRequest.rotation = turnRot;
            moveRequest.rotation.Normalize();

			AddMovement(ref moveRequest);

    if(!m_leftTrack.IsDestroyed)
                m_leftTrack.Material = GetTrackMaterial(m_acceleration.X);
            if(!m_rightTrack.IsDestroyed)
                m_rightTrack.Material = GetTrackMaterial(m_acceleration.Y);
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
            if (m_tankInput == null)
                return;

            var accelerationSpeed = GameCVars.tank_accelerationSpeed * frameTime;
            var accelerationSpeedRotation = GameCVars.tank_accelerationSpeedRotation * frameTime;

            var maxAcceleration = GameCVars.tank_maxAcceleration;
            if (m_tankInput.HasFlag(InputFlags.Boost))
                maxAcceleration = GameCVars.tank_maxAccelerationBoosting;

            // in order to make the tank feel heavy, hinder forward / backwards movement when attempting to turn.
            if (m_tankInput.HasFlag(InputFlags.MoveLeft))
            {
                m_acceleration.X += accelerationSpeedRotation;
                m_acceleration.Y -= accelerationSpeedRotation;
            }
            else if (m_tankInput.HasFlag(InputFlags.MoveRight))
            {
                m_acceleration.X -= accelerationSpeedRotation;
                m_acceleration.Y += accelerationSpeedRotation;
            }
            else if (m_tankInput.HasFlag(InputFlags.MoveForward))
            {
                m_acceleration.X += accelerationSpeed;
                m_acceleration.Y += accelerationSpeed;
            }
            else if (m_tankInput.HasFlag(InputFlags.MoveBack))
            {
                m_acceleration.X -= accelerationSpeed;
                m_acceleration.Y -= accelerationSpeed;
            }
            else
            {
                MathHelpers.Interpolate(ref m_acceleration.X, 0, GameCVars.tank_decelerationSpeed * frameTime);
                MathHelpers.Interpolate(ref m_acceleration.Y, 0, GameCVars.tank_decelerationSpeed * frameTime);
            }

            m_acceleration.X = MathHelpers.Clamp(m_acceleration.X, -maxAcceleration, maxAcceleration);
            m_acceleration.Y = MathHelpers.Clamp(m_acceleration.Y, -maxAcceleration, maxAcceleration);
        }

        Vec2 m_acceleration = new Vec2();
	}
}