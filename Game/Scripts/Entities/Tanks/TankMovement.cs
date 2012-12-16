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
			if(IsDestroyed)
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

            var trackMoveDirection = FilterMovement(m_trackMoveDirection);

            var groundFriction = Physics.Status.Living.GroundSurfaceType.Parameters.Friction;

            var onGround = !Physics.Status.Living.IsFlying;

            var slopeAngle = onGround ? NormalToAngle(GroundNormal) : MathHelpers.DegreesToRadians(90);

            ///////////////////////////
            // Velocity
            ///////////////////////////

            var acceleration = trackMoveDirection.X + trackMoveDirection.Y;
            var forwardAcceleration = forwardDir * acceleration * GameCVars.movementSpeedMultiplier;

            var frictionDeceleration = normalizedVelocity * (float)(groundFriction * Math.Abs(CVar.Get("p_gravity_z").FVal) * Math.Cos(slopeAngle));
            var dragDeceleration = (1.2f * 1.27f * (normalizedVelocity * (float)Math.Pow(prevVelocity.Length, 2))) / Physics.Status.Dynamics.Mass;

            moveRequest.velocity = prevVelocity + (forwardAcceleration - frictionDeceleration - dragDeceleration) * Time.DeltaTime;
            if (moveRequest.velocity.Length > 0 && onGround)
                moveRequest.velocity *= MathHelpers.Min(forwardDir.Dot(normalizedVelocity) * GameCVars.turnModifier, 1.0f);

            ///////////////////////////
            // Rotation
            ///////////////////////////

            // turning
            float angleChange = ((trackMoveDirection.X - trackMoveDirection.Y) / 2) * Time.DeltaTime;

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

        private void OnRotateRight(ActionMapEventArgs e)
        {
            m_trackMoveDirection.Y = e.Value;

            if (GameCVars.hardcoreMode == 0)
                m_trackMoveDirection.X = -e.Value;
        }

        private void OnRotateLeft(ActionMapEventArgs e)
        {
            m_trackMoveDirection.X = e.Value;

            if (GameCVars.hardcoreMode == 0)
                m_trackMoveDirection.Y = -e.Value;
        }

        #region Hardcore mode only
        private void OnRotateRightReverse(ActionMapEventArgs e)
        {
            if (GameCVars.hardcoreMode == 1)
                m_trackMoveDirection.Y = -e.Value;
        }

        private void OnRotateLeftReverse(ActionMapEventArgs e)
        {
            if (GameCVars.hardcoreMode == 1)
                m_trackMoveDirection.X = -e.Value;
        }
        #endregion

        private void OnMoveForward(ActionMapEventArgs e)
		{
            if (GameCVars.hardcoreMode == 0)
            {
                m_trackMoveDirection.X = e.Value;
                m_trackMoveDirection.Y = e.Value;
            }
		}

		private void OnMoveBack(ActionMapEventArgs e)
		{
            if (GameCVars.hardcoreMode == 0)
            {
                m_trackMoveDirection.X = -e.Value;
                m_trackMoveDirection.Y = -e.Value;
            }
		}

		private void OnSprint(ActionMapEventArgs e)
		{
		}

        void ApplyMovement(Vec2 delta)
        {
            m_trackMoveDirection.X = MathHelpers.Clamp(m_trackMoveDirection.X + delta.X, -1.0f, 1.0f);
            m_trackMoveDirection.Y = MathHelpers.Clamp(m_trackMoveDirection.Y + delta.Y, -1.0f, 1.0f);
        }

        Vec2 FilterMovement(Vec2 desired)
        {
            float frameTimeCap = MathHelpers.Min(Time.DeltaTime, 0.033f);
	        float inputAccel = 30;

	        var oldFilteredMovement = m_filteredMoveDirection;

	        if (desired.Length < 0.01f)
		        m_filteredMoveDirection = new Vec2(0, 0);
	        else if (inputAccel<=0.0f)
		        m_filteredMoveDirection = desired;
	        else
	        {
		        var delta = desired - m_filteredMoveDirection;

		        float len = delta.Length;
		        if (len<=1.0f)
			        delta = delta * (1.0f - len*0.55f);

		        m_filteredMoveDirection += delta * MathHelpers.Min(frameTimeCap * inputAccel, 1.0f);
	        }

            return m_filteredMoveDirection;
        }

        /// <summary>
        /// -1 - 1, -1 if going backwards, 0 if still, 1 if forward.
        /// x = left, y = right
        /// </summary>
        Vec2 m_trackMoveDirection;

        Vec2 m_filteredMoveDirection;
	}
}