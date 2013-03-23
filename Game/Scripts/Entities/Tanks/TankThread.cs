using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CryEngine;

namespace CryGameCode.Tanks
{
    class TankThread
    {
        TankThread() { }

        public TankThread(Tank owner, Vec2 pos)
        {
            Owner = owner;
            LocalPos = pos;
        }

        private float m_frictionCoefficient = 0.8f; //Asphalt to Rubber
        
        public Vec2 LocalPos { get; set; }
        public Tank Owner { get; private set; }
        public float Force { get; set; }
        public float Power { get; set; } //kW
    }
}
