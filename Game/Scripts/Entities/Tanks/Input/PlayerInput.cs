using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CryEngine;
using CryEngine.Extensions;
using CryEngine.Serialization;

namespace CryGameCode.Tanks
{
    public class PlayerInput
    {
        // Default constructor for realtime scripting 
        PlayerInput() { }

        public PlayerInput(Tank tank)
        {
            Owner = tank;

            AddEvent("zoom_in", InputFlags.ZoomIn);
            AddEvent("zoom_out", InputFlags.ZoomOut);

            AddEvent("moveright", InputFlags.MoveRight);
            AddEvent("moveleft", InputFlags.MoveLeft);

            AddEvent("moveforward", InputFlags.MoveForward);
            AddEvent("moveback", InputFlags.MoveBack);

            AddEvent("sprint", InputFlags.Boost);

            Input.ActionmapEvents.Add("attack1", (e) =>
                {
                    if (e.KeyEvent == KeyEvent.OnRelease)
                    {
                        var gameRules = GameRules.Current as SinglePlayer;

                        if (Owner != null && Owner.IsDead)
                        {
                            if (Network.IsServer)
                                gameRules.RequestRevive(Owner.Id, Owner.Team, Owner.TurretTypeName);
                            else
                            {
                                Debug.LogAlways("Requesting revive ({0}, {1}, {2})", Owner.Id, Owner.Team, Owner.Turret.GetType().Name);
                                Owner.RemoteInvocation(gameRules.RequestRevive, NetworkTarget.ToServer, Owner.Id, Owner.Team, Owner.TurretTypeName);
                            }
                        }
                        else if (Owner == null)
                            Debug.LogAlways("Could not request revive, owner as null");
                        else if (Owner.IsDead)
                            Debug.LogAlways("Could not request revive, owner was alive.");
                    }
                });

            Input.ActionmapEvents.Add("rotateyaw", (e) =>
                {
                    if (Owner != null && Owner.Turret != null)
                        Owner.Turret.OnRotateYaw(e.Value);
                });

            Input.ActionmapEvents.Add("rotatepitch", (e) =>
                {
                    if (Owner != null && Owner.Turret != null)
                        Owner.Turret.OnRotatePitch(e.Value);
                });

            Input.ActionmapEvents.Add("cycle_view", (e) =>
                {
                    if (GameCVars.cam_type < (int)CameraType.Last - 1)
                        GameCVars.cam_type++;
                    else
                        GameCVars.cam_type = 0;
                });
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
