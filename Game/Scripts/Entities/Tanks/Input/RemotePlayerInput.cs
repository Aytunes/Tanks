using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CryEngine;
using CryEngine.Extensions;
using CryEngine.Serialization;

namespace CryGameCode.Tanks
{
    public class RemotePlayerInput : IPlayerInput
    {
        public RemotePlayerInput(Tank tank)
        {
            Owner = tank;
        }

        public void PreUpdate() 
        {

        }

        public void Update() { }
        public void PostUpdate() { }
        public void Reset() { }

        public void NetSerialize(CryEngine.Serialization.CrySerialize serialize)
        {
        }

        public void Destroy()
        {
        }

        public bool HasFlag(InputFlags flag)
        {
            // Enum.HasFlag is very slow, avoid usage.
            return ((Flags & flag) == flag);
        }

        public InputFlags Flags { get; set; }
        public Actor Owner { get; private set; }
    }
}
