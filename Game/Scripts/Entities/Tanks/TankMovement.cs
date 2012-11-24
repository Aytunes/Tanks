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

			// dampen the movement.
			MathHelpers.Interpolate(ref m_rotation, 0, rotationDamping * Time.DeltaTime);
            MathHelpers.Interpolate(ref velocity, Vec3.Zero, movementDamping * Time.DeltaTime);
            if (!Physics.LivingStatus.IsFlying)
            {
                var acceleration = LocalRotation.Column1 * m_acceleration * speedMult;
                if(velocity != Vec3.Zero)
                    velocity *= LocalRotation.Column1.Dot(velocity.Normalized);

                moveRequest.velocity = velocity + acceleration;
            }

            m_acceleration = 0;

			moveRequest.rotation = LocalRotation;
			moveRequest.rotation.SetRotationXYZ(new Vec3(0, 0, m_rotation) * Time.DeltaTime);
			moveRequest.rotation = moveRequest.rotation.Normalized;

			AddMovement(ref moveRequest);

			var mat = Material.Find(moveRequest.velocity.Length > 0.3f ? "objects/tanks/tracksmoving" : "objects/tanks/tracks");
			if(mat != null && !m_leftTrack.IsDestroyed && !m_rightTrack.IsDestroyed)
			{
				m_leftTrack.Material = mat;
				m_rightTrack.Material = mat;
			}
		}


		private void OnMoveRight(ActionMapEventArgs e)
		{
			m_rotation = MathHelpers.Clamp(m_rotation - RotationSpeed * Time.DeltaTime, -m_maxRotationSpeed, m_maxRotationSpeed);
		}

		private void OnMoveLeft(ActionMapEventArgs e)
		{
			m_rotation = MathHelpers.Clamp(m_rotation + RotationSpeed * Time.DeltaTime, -m_maxRotationSpeed, m_maxRotationSpeed);
		}

		private void OnMoveForward(ActionMapEventArgs e)
		{
            m_acceleration = e.Value;//MathHelpers.Clamp(m_acceleration + TankSpeed, -m_maxSpeed, m_maxSpeed);
		}

		private void OnMoveBack(ActionMapEventArgs e)
		{

            m_acceleration = -e.Value;//MathHelpers.Clamp(m_acceleration - TankSpeed, -m_maxSpeed, m_maxSpeed);
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
		private const float m_maxSpeed = 8f;
		private const float m_maxRotationSpeed = 1.5f;

		public bool IsBoosting { get; set; }
		public float BoostTime { get; set; }

		public float SpeedMultiplier { get; set; }
        public float BackwardsSpeedMultiplier { get; set; }
		public virtual float RotationSpeed { get { return 10f; } }
	}
}