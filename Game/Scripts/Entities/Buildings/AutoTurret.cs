using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CryEngine;

using CryGameCode.Tanks;

namespace CryGameCode.Entities.Buildings
{
    [Entity(Category = "Buildings")]
    public class AutoTurret : Entity
    {
        protected override void OnEditorReset(bool enteringGame)
        {
            Reset();
        }

        public override void OnSpawn()
        {
            Reset();
        }

        protected override void PostSerialize()
        {
            Reset();
        }

        void Reset()
        {
            ReceiveUpdates = true;

            Range = 500;
        }

        public override void OnUpdate()
        {
            var position = Position;

            var bbox = new BoundingBox(new Vec3(position.X - Range, position.Y - Range, position.Z - Range), new Vec3(position.X + Range, position.Y + Range, position.Z + Range));

            var possibleTargets = Entity.QueryProximity<Tank>(bbox);

            float closestDistanceSquared = Range * Range;
            Tank closestTank = null;

            foreach(var tank in possibleTargets)
            {
                var tankPosition = tank.Position;
                Vec3 deltaDist = tankPosition - position;

                float distanceSquared = deltaDist.LengthSquared;

                if (distanceSquared < closestDistanceSquared)
                {
                    closestTank = tank;
                    closestDistanceSquared = distanceSquared;
                }
            }

            if(closestTank != null)
                FireAt(closestTank);
        }

        void FireAt(EntityBase target)
        {
            if (Time.FrameStartTime > lastShot + (TimeBetweenShots * 1000))
            {
                lastShot = Time.FrameStartTime;

                var turretPos = Position + new Vec3(0, 0, 1);
                Vec3 direction = target.Position - Position;
                direction.Normalize();

                Entity.Spawn<Projectiles.Bullet>("pain", turretPos, Quat.CreateRotationVDir(direction));
            }
        }

        float lastShot;
        float TimeBetweenShots { get { return 0.1f; } }

        public float Range { get; set; }
    }
}
