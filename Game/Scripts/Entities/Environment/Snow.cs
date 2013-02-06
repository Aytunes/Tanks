using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CryEngine;

namespace CryGameCode.Entities.Environment
{
    [Entity(Category = "Environment", Icon = "shake.bmp")]
    public class Snow : Entity
    {
        public override void OnSpawn()
        {
            Enabled = true;
            Radius = 50;

            SnowFlakeCount = 100;
            SnowFlakeSize = 1;

            Brightness = 1;
            GravityScale = 0.1f;
            WindScale = 0.1f;
            TurbulenceStrength = 0.5f;
            TurbulenceFrequency = 0.1f;

            ReceiveUpdates = true;
        }

        public override void OnUpdate()
        {
            if (Enabled)
            {
                Cry3DEngine.SetSnowSurfaceParams(Position, Radius, SnowAmount, FrostAmount, SurfaceFreezing);
                Cry3DEngine.SetSnowFallParams(SnowFlakeCount, SnowFlakeSize, Brightness, GravityScale, WindScale, TurbulenceStrength, TurbulenceFrequency);
            }
            else
            {
                Cry3DEngine.SetSnowSurfaceParams(Position, 0, 0, 0, 0);
                Cry3DEngine.SetSnowFallParams(0, 0, 0, 0, 0, 0, 0);
            }
        }

        protected override bool OnRemove()
        {
            Cry3DEngine.SetSnowSurfaceParams(Vec3.Zero, 0, 0, 0, 0);
            Cry3DEngine.SetSnowFallParams(0, 0, 0, 0, 0, 0, 0);

            return true;
        }

        protected override void FullSerialize(CryEngine.Serialization.CrySerialize serialize)
        {
            // TODO: Serialize all properties
        }

        [EditorProperty]
        public bool Enabled { get; set; }

        [EditorProperty]
        public float Radius { get; set; }

        [EditorProperty(Folder = "Surface")]
        public float SnowAmount { get; set; }

        [EditorProperty(Folder = "Surface")]
        public float FrostAmount { get; set; }

        [EditorProperty(Folder = "Surface")]
        public float SurfaceFreezing { get; set; }

        [EditorProperty(Folder = "SnowFall")]
        public int SnowFlakeCount { get; set; }

        [EditorProperty(Folder = "SnowFall")]
        public int SnowFlakeSize { get; set; }

        [EditorProperty(Folder = "SnowFall")]
        public float Brightness { get; set; }

        [EditorProperty(Folder = "SnowFall")]
        public float GravityScale { get; set; }

        [EditorProperty(Folder = "SnowFall")]
        public float WindScale { get; set; }

        [EditorProperty(Folder = "SnowFall")]
        public float TurbulenceStrength { get; set; }

        [EditorProperty(Folder = "SnowFall")]
        public float TurbulenceFrequency { get; set; }
    }
}
