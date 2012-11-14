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

			// dampen the movement. 
			MathHelpers.Interpolate(ref m_rotation, 0, rotationDamping * Time.DeltaTime);
			MathHelpers.Interpolate(ref m_acceleration, 0, movementDamping * Time.DeltaTime);

			RotationRequest = new Vec3(0, 0, m_rotation);

			VelocityRequest = LocalRotation.Column1 * m_acceleration * speedMult;

			if(!Physics.LivingStatus.IsFlying)
				moveRequest.velocity = VelocityRequest;

			moveRequest.rotation = LocalRotation;
			moveRequest.rotation.SetRotationXYZ(RotationRequest * Time.DeltaTime);
			moveRequest.rotation = moveRequest.rotation.Normalized;

			AddMovement(ref moveRequest);

			// reset movement vectors
			VelocityRequest = Vec3.Zero;
			RotationRequest = Vec3.Zero;

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
			m_acceleration = MathHelpers.Clamp(m_acceleration + TankSpeed * Time.DeltaTime, -m_maxSpeed, m_maxSpeed);
		}

		private void OnMoveBack(ActionMapEventArgs e)
		{
			m_acceleration = MathHelpers.Clamp(m_acceleration - TankSpeed * Time.DeltaTime, -m_maxSpeed, m_maxSpeed);
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

		protected Vec3 VelocityRequest;
		protected Vec3 RotationRequest;

		public float SpeedMultiplier { get; set; }
		public virtual float TankSpeed { get { return 24f; } }
		public virtual float RotationSpeed { get { return 10f; } }
	}
}