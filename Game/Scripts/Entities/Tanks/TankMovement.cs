using System;
using CryEngine;

namespace CryGameCode.Tanks
{
    public partial class Tank
    {


        private float movementDamping = 50f;
        private float rotationDamping = 350f;


        protected override void OnPrePhysicsUpdate()
        {
            if (IsDestroyed)
                return;

            var moveRequest = new EntityMovementRequest();
            moveRequest.type = EntityMoveType.Normal;

            // dampen the movement. 
            MathHelpers.Interpolate(ref m_rotation, 0, rotationDamping * Time.DeltaTime);
            MathHelpers.Interpolate(ref m_acceleration, 0, movementDamping * Time.DeltaTime);

            RotationRequest = new Vec3(0, 0, m_rotation * Math.Sign(m_acceleration));

            VelocityRequest = LocalRotation.Column1 * m_acceleration * SpeedMultiplier;

            if (!Physics.LivingStatus.IsFlying)
                moveRequest.velocity = VelocityRequest;

            moveRequest.rotation = LocalRotation;
            moveRequest.rotation.SetRotationXYZ(RotationRequest * Time.DeltaTime);
            moveRequest.rotation = moveRequest.rotation.Normalized;

            AddMovement(ref moveRequest);

            // reset movement vectors
            VelocityRequest = Vec3.Zero;
            RotationRequest = Vec3.Zero;

            if (moveRequest.velocity.Length > 0.3f) // it'd be nice if we could change the oscillation speed / dir 
            {
                var moveMat = Material.Find("objects/tanks/tracksmoving");
                if (moveMat != null && !m_leftTrack.IsDestroyed && !m_rightTrack.IsDestroyed)
                {
                    m_leftTrack.Material = moveMat;
                    m_rightTrack.Material = moveMat;
                }
            }
            else
            {
                var defaultMat = Material.Find("objects/tanks/tracks");
                if (defaultMat != null && !m_leftTrack.IsDestroyed && !m_rightTrack.IsDestroyed)
                {
                    m_leftTrack.Material = defaultMat;
                    m_rightTrack.Material = defaultMat;
                }
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
            if (e.KeyEvent == KeyEvent.OnPress)
                SpeedMultiplier = 1.5f;
            else if (e.KeyEvent == KeyEvent.OnRelease)
                SpeedMultiplier = 1;
        }


        private float m_acceleration;
        private float m_rotation;
        private const float m_maxSpeed = 8f;
        private const float m_maxRotationSpeed = 1f;

        protected Vec3 VelocityRequest;
        protected Vec3 RotationRequest;

        static float tankTurnSpeed = 2;

        public float SpeedMultiplier = 1.0f;
        public virtual float TankSpeed { get { return 24f; } }
        public virtual float RotationSpeed { get { return 10f; } }
    }
}