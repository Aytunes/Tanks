using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CryEngine;
using CryEngine.Extensions;
using CryEngine.Serialization;

namespace CryGameCode.Tanks
{
    public class TankInput
    {
        // Default constructor for realtime scripting 
        TankInput() { }

        public TankInput(Tank tank)
        {
            Owner = tank;

            if (tank.IsLocalClient)
            {
                AddEvent("zoom_in", InputFlags.ZoomIn);
                AddEvent("zoom_out", InputFlags.ZoomOut);

                AddEvent("moveright", InputFlags.MoveRight);
                AddEvent("moveleft", InputFlags.MoveLeft);

                AddEvent("moveforward", InputFlags.MoveForward);
                AddEvent("moveback", InputFlags.MoveBack);

                AddEvent("sprint", InputFlags.Boost);
            }
        }

        void AddEvent(string actionMapName, InputFlags flag)
        {
            Input.ActionmapEvents.Add(actionMapName, (e) => { SetFlag(flag, e.KeyEvent); });
        }

        void NetSerialize(CrySerialize serialize)
        {
            serialize.BeginGroup("TankInput");

            int flags = (int)m_flags;
            serialize.EnumValue("m_flags", ref flags, (int)InputFlags.First, (int)InputFlags.Last);
            if (!serialize.IsReading())
                m_flags = (InputFlags)flags;

            serialize.EndGroup();
        }

        public Tank Owner { get; private set; }

        private InputFlags m_flags;

        public void Destroy()
        {
            if(Owner.IsLocalClient)
            Input.ActionmapEvents.RemoveAll(this);
        }

        void SetFlag(InputFlags flag, KeyEvent keyEvent)
        {
            var hasFlag = HasFlag(flag);

            switch (keyEvent)
            {
                case KeyEvent.OnPress:
                    {
                        if (!hasFlag)
                            m_flags |= flag;
                    }
                    break;
                case KeyEvent.OnRelease:
                    {
                        if (hasFlag)
                        {
                            m_flags &= ~flag;
                        }
                    }
                    break;
            }
        }

        public bool HasFlag(InputFlags flag)
        {
            // Enum.HasFlag is very slow, avoid usage.
            return ((m_flags & flag) == flag);
        }
    }
}
