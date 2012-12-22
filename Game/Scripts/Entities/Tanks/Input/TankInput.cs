using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CryEngine;
using CryEngine.Extensions;

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
                Input.ActionmapEvents.Add("zoom_in", OnZoomIn);
                Input.ActionmapEvents.Add("zoom_out", OnZoomOut);

                Input.ActionmapEvents.Add("moveright", OnMoveRight);
                Input.ActionmapEvents.Add("moveleft", OnMoveLeft);

                Input.ActionmapEvents.Add("sprint", OnBoost);

                Input.ActionmapEvents.Add("moveforward", OnMoveForward);
                Input.ActionmapEvents.Add("moveback", OnMoveBack);
            }
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

        #region Camera
        private void OnZoomIn(ActionMapEventArgs e)
        {
            SetFlag(InputFlags.ZoomIn, e.KeyEvent);
        }

        private void OnZoomOut(ActionMapEventArgs e)
        {
            SetFlag(InputFlags.ZoomOut, e.KeyEvent);
        }
        #endregion

        #region Movement
        private void OnMoveRight(ActionMapEventArgs e)
        {
            SetFlag(InputFlags.MoveRight, e.KeyEvent);
        }

        private void OnMoveLeft(ActionMapEventArgs e)
        {
            SetFlag(InputFlags.MoveLeft, e.KeyEvent);
        }

        private void OnMoveForward(ActionMapEventArgs e)
        {
            SetFlag(InputFlags.MoveForward, e.KeyEvent);
        }

        private void OnMoveBack(ActionMapEventArgs e)
        {
            SetFlag(InputFlags.MoveBack, e.KeyEvent);
        }

        private void OnBoost(ActionMapEventArgs e)
        {
            SetFlag(InputFlags.Boost, e.KeyEvent);
        }
        #endregion
    }
}
