using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryGameCode.Tanks
{
    [Flags]
    public enum InputFlags
    {
        ZoomIn = 1 << 1,
        ZoomOut = 1 << 2,

        MoveRight = 1 << 3,
        MoveLeft = 1 << 4,

        MoveForward = 1 << 5,
        MoveBack = 1 << 6,

        Boost = 1 << 7,
    }
}
