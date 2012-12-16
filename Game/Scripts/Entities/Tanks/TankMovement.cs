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
            if (IsDestroyed || m_tankInput == null)
				return;

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

            var trackMoveDirection = GetTrackMoveDirection();

            ///////////////////////////
            // Velocity
            ///////////////////////////

            var acceleration = trackMoveDirection.X + trackMoveDirection.Y;
            var forwardAcceleration = forwardDir * acceleration * GameCVars.tank_movementSpeedMult * travelDirectionDot;

            var frictionDeceleration = normalizedVelocity * (float)(groundFriction * Math.Abs(CVar.Get("p_gravity_z").FVal) * Math.Cos(slopeAngle)) * GameCVars.tank_movementFrictionMult;

            var dragDeceleration = (1.2f * 0.588f * 1.27f * (normalizedVelocity * (float)Math.Pow(prevVelocity.Length, 2))) / Physics.Status.Dynamics.Mass;

            moveRequest.velocity = prevVelocity + (forwardAcceleration - frictionDeceleration - dragDeceleration) * Time.DeltaTime;
            A
            ///////////////////////////
            // Rotation
            ///////////////////////////

            // turning
            float angleChange = ((trackMoveDirection.X - trackMoveDirection.Y) / 2) * Time.DeltaTime * GameCVars.tank_rotationSpeed;

            var turnRot = Quat.CreateRotationZ(angleChange);

            moveRequest.rotation = turnRot;
            moveRequest.rotation.Normalize();

			AddMovement(ref moveRequest);

            if(!m_leftTrack.IsDestroyed)
                m_leftTrack.Material = GetTrackMaterial(trackMoveDirection.X);
            if(!m_rightTrack.IsDestroyed)
                m_rightTrack.Material = GetTrackMaterial(trackMoveDirection.Y);
		}

        private Material GetTrackMaterial(float moveDirection)
        {
            if (moveDirection < 0)
                return Material.Find("objects/tanks/tracksmoving_back");
            else if(moveDirection == 0)
                return Material.Find("objects/tanks/tracks");

            return Material.Find("objects/tanks/tracksmoving_forward");
        }

        Vec2 GetTrackMoveDirection()
        {
            Vec2 moveDirection = new Vec2(0, 0);

            var boostMultiplier = 1;
            if (m_tankInput.HasFlag(InputFlags.Boost))
                boostMultiplier = 2;

            if (m_tankInput.HasFlag(InputFlags.MoveForward))
                moveDirection.X = moveDirection.Y = 1 * boostMultiplier;
            else if (m_tankInput.HasFlag(InputFlags.MoveBack))
                moveDirection.X = moveDirection.Y  = -1 * boostMultiplier;
            
            if (m_tankInput.HasFlag(InputFlags.MoveLeft))
            {
                moveDirection.X += 1;
                moveDirection.Y += -1;
            }
            else if (m_tankInput.HasFlag(InputFlags.MoveRight))
            {
                moveDirection.X += -1;
                moveDirection.Y += 1;
            }

            return moveDirection;
        }
	}
}