using System;
using CryEngine;

namespace CryGameCode.Tanks
{
	public partial class Tank
	{
		private const float movementDamping = 350;
		private const float rotationDamping = 350;

		protected override void OnPrePhysicsUpdate()
		{
			if(IsDestroyed)
				return;

			var speedMult = SpeedMultiplier;
			if(IsBoosting && BoostTime > 0)
				speedMult *= boostSpeedMult;

			var moveRequest = new EntityMovementRequest();
			moveRequest.type = EntityMoveType.Normal;

            Vec3 velocity = Velocity;

            Quat localRotation = LocalRotation;

			// dampen the movement.
			MathHelpers.Interpolate(ref m_rotation, 0, rotationDamping * Time.DeltaTime);
            MathHelpers.Interpolate(ref velocity, Vec3.Zero, movementDamping * Time.DeltaTime);
            if (!Physics.LivingStatus.IsFlying)
            {
                var acceleration = localRotation.Column1 * m_acceleration * speedMult;
                if(velocity != Vec3.Zero)
                    velocity *= localRotation.Column1.Dot(velocity.Normalized);

                moveRequest.velocity = velocity + acceleration * Time.DeltaTime;
            }

            m_acceleration = 0;

            var turn = localRotation.Column2 * m_rotation * Time.DeltaTime;

            moveRequest.rotation = Quat.CreateRotationXYZ(turn);
            moveRequest.rotation.Normalize();

			AddMovement(ref moveRequest);

			var mat = Material.Find(moveRequest.velocity.Length > 0.3f ? "objects/tanks/tracksmoving" : "objects/tanks/tracks");
			if(mat != null && !m_leftTrack.IsDestroyed && !m_rightTrack.IsDestroyed)
			{
				m_leftTrack.Material = mat;
				m_rightTrack.Material = mat;
			}
		}


		private void OnRotateRight(ActionMapEventArgs e)
		{
            m_rotation = -e.Value * m_maxRotationSpeed;
		}

		private void OnRotateLeft(ActionMapEventArgs e)
		{
            m_rotation = e.Value * m_maxRotationSpeed;
		}

		private void OnMoveForward(ActionMapEventArgs e)
		{
            m_acceleration = e.Value * m_maxSpeed;
		}

		private void OnMoveBack(ActionMapEventArgs e)
		{
            m_acceleration = -e.Value * m_maxSpeed;
		}

		private void OnSprint(ActionMapEventArgs e)
		{
			if(e.KeyEvent == KeyEvent.OnPress)
				IsBoosting = true;
			else if(e.KeyEvent == KeyEvent.OnRelease)
				IsBoosting = false;
		}


		private float m_acceleration;
		private float m_rotation;
		private const float m_maxSpeed = 50f;
		private const float m_maxRotationSpeed = 1.5f;

		public bool IsBoosting { get; set; }
		public float BoostTime { get; set; }

		public float SpeedMultiplier { get; set; }
        public float BackwardsSpeedMultiplier { get; set; }
		public virtual float RotationSpeed { get { return 10f; } }
	}
}