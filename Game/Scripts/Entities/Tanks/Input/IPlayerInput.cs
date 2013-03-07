using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CryEngine;

namespace CryGameCode.Tanks
{
    public interface IPlayerInput
    {
        void PreUpdate();
        void Update();
        void PostUpdate();

        void NetSerialize(CryEngine.Serialization.CrySerialize serialize);

        void Destroy();

        bool HasFlag(InputFlags flag);

        InputFlags Flags { get; set; }
        Actor Owner { get; }
    }
}
