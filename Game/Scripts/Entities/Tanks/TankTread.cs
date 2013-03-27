using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CryEngine;

namespace CryGameCode.Tanks
{
    class TankTread
    {

        public TankTread(Tank owner, Vec2 pos, float maxForce = 150000.0f)
        {
            Owner = owner;
            LocalPos = pos;
            m_maxForce = maxForce;
        }

        public void Update( )
        {
            Force = m_maxForce * m_throttle;
        }

        public void SetThrottle(float value, float speed = 0.0f)
        {
            m_throttle = MathHelpers.Clamp(value, -1.0f, 1.0f);
        }

        public float GetThrottle()
        {
            return m_throttle;
        }

        private float m_throttle = 0.0f;
        private float m_maxForce = 0.0f;
        
        public Vec2 LocalPos { get; set; }
        public Tank Owner { get; private set; }
        public float Force { get; private set; }
    }
}
