using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CryEngine;
using CryEngine.Extensions;
using CryEngine.Serialization;

namespace CryGameCode.Tanks
{
    public class PlayerInput : IPlayerInput
    {
        // Default constructor for realtime scripting 
        PlayerInput() { }

        public PlayerInput(Tank tank)
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

                Input.ActionmapEvents.Add("attack1", (e) =>
                    {
                        if (e.KeyEvent == KeyEvent.OnRelease)
                        {
                            var gameRules = GameRules.Current as SinglePlayer;

                            if (tank != null && tank.IsDead)
                            {
                                if (Network.IsServer)
                                    gameRules.RequestRevive(tank.Id, tank.Team, tank.TurretTypeName);
                                else
                                
                                    Debug.LogAlways("Requesting revive ({0}, {1}, {2})", tank.Id, tank.Team, tank.TurretTypeName);
                                    Owner.RemoteInvocation(gameRules.RequestRevive, NetworkTarget.ToServer, tank.Id, tank.Team, tank.TurretTypeName);
                                }
                            }
                            else if (Owner == null)
                                Debug.LogAlways("Could not request revive, owner as null");
                            else if (Owner.IsDead)
                                Debug.LogAlways("Could not request revive, owner was alive.");
                        }
                    });

                Input.ActionmapEvents.Add("cycle_view", (e) =>
                    {
                        if (GameCVars.cam_type < (int)CameraType.Last - 1)
                            GameCVars.cam_type++;
                        else
                            GameCVars.cam_type = 0;
                    });
            }
        }

        public void PreUpdate() { }
        public void Update() { }
        public void PostUpdate() { }

        public void NetSerialize(CryEngine.Serialization.CrySerialize serialize)
        {
            serialize.BeginGroup("PlayerInput");

            Debug.LogAlways("Flags was {0}", Flags);
            int flags = (int)m_flags;
            serialize.Value("m_flags", ref flags);
            m_flags = (InputFlags)flags;
            Debug.LogAlways("Flags is {0}", Flags);

            serialize.EndGroup();
        }

        void AddEvent(string actionMapName, InputFlags flag)
        {
            Input.ActionmapEvents.Add(actionMapName, (e) => { SetFlag(flag, e.KeyEvent); });
        }

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
                        {
                            Flags |= flag;

                            Owner.GameObject.NotifyNetworkStateChange(Aspect);
                        }
                    }
                    break;
                case KeyEvent.OnRelease:
                    {
                        if (hasFlag)
                        {
                            Flags &= ~flag;

                            Owner.GameObject.NotifyNetworkStateChange(Aspect);
                        }
                    }
                    break;
            }
        }

        public bool HasFlag(InputFlags flag)
        {
            // Enum.HasFlag is very slow, avoid usage.
            return ((Flags & flag) == flag);
        }

        public static int Aspect = 256;

        InputFlags m_flags;
        public InputFlags Flags { get { return m_flags; } set { m_flags = value; } }
        public Actor Owner { get; private set; }
    }
}
