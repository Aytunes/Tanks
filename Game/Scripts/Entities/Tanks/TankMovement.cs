using CryEngine;

namespace CryGameCode.Tanks
{
	public partial class Tank
	{
		protected override void OnPrePhysicsUpdate()
		{
			if(IsDestroyed)
				return;

			var moveRequest = new EntityMovementRequest();
			moveRequest.type = EntityMoveType.Normal;

			if(!Physics.LivingStatus.IsFlying)
				moveRequest.velocity = VelocityRequest;

			moveRequest.rotation = LocalRotation;
			moveRequest.rotation.SetRotationXYZ(RotationRequest * Time.DeltaTime);
			moveRequest.rotation = moveRequest.rotation.Normalized;

			AddMovement(ref moveRequest);

			VelocityRequest = Vec3.Zero;
			RotationRequest = Vec3.Zero;

			if(moveRequest.velocity != Vec3.Zero)
			{
				var moveMat = Material.Find("objects/tanks/tracksmoving");
				if(moveMat != null && !m_leftTrack.IsDestroyed && !m_rightTrack.IsDestroyed)
				{
					m_leftTrack.Material = moveMat;
					m_rightTrack.Material = moveMat;
				}
			}
			else
			{
				var defaultMat = Material.Find("objects/tanks/tracks");
				if(defaultMat != null && !m_leftTrack.IsDestroyed && !m_rightTrack.IsDestroyed)
				{
					m_leftTrack.Material = defaultMat;
					m_rightTrack.Material = defaultMat;
				}
			}
		}

		private void OnMoveRight(ActionMapEventArgs e)
		{
			RotationRequest.Z -= tankTurnSpeed;
		}

		private void OnMoveLeft(ActionMapEventArgs e)
		{
			RotationRequest.Z += tankTurnSpeed;
		}

		private void OnMoveForward(ActionMapEventArgs e)
		{
			VelocityRequest += LocalRotation.Column1 * TankSpeed * SpeedMultiplier;
		}

		private void OnMoveBack(ActionMapEventArgs e)
		{
			VelocityRequest += LocalRotation.Column1 * -TankSpeed * SpeedMultiplier;
		}

		private void OnSprint(ActionMapEventArgs e)
		{
			if(e.KeyEvent == KeyEvent.OnPress)
				SpeedMultiplier = 1.5f;
			else if(e.KeyEvent == KeyEvent.OnRelease)
				SpeedMultiplier = 1;
		}

		protected Vec3 VelocityRequest;
		protected Vec3 RotationRequest;

		static float tankTurnSpeed = 2;

		public float SpeedMultiplier = 1.0f;
		public virtual float TankSpeed { get { return 10; } }
	}
}
