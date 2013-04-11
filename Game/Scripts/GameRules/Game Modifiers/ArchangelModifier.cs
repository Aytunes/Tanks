using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CryEngine;
using CryGameCode.Entities;

namespace CryGameCode
{
    public class ArchangelModifier : IGameModifier
    {
        ArchangelModifier() { }

        public ArchangelModifier(Vec3 position, Vec3 direction, float relativeDamage, float minRadius, float radius, float maxRadius)
        {
            var hits = Ray.Cast(position, direction);
            // TODO: Spawn archangel laser-style particle from position to the hit point.

            if (hits.Count() > 0)
            {
                var hit = hits.ElementAt(0);

                var explosion = new Explosion
                {
                    Epicenter = Position,
                    EpicenterImpulse = Position,
                    Direction = direction,
                    MinRadius = minRadius,
                    Radius = radius,
                    MaxRadius = maxRadius,
                    ImpulsePressure = 0.0f//ExplosionPressure Disabled for now causes movement bugs
                };

                explosion.Explode();

                foreach (var affectedPhysicalEntity in explosion.AffectedEntities)
                {
                    var entity = affectedPhysicalEntity.Owner;
                    var damageable = entity as IDamageable;
                    if (damageable == null)
                        continue;

                    var distance = System.Math.Abs((Position - entity.Position).Length);

                    var damage = relativeDamage * (1 - (distance / maxRadius));

                    damageable.Damage(0, damage, DamageType.Explosive, Vec3.Zero, direction);
                }
            }
        }

        public bool Update()
        {
            return true;
        }

        public EntityBase Target { get; set; }

        public Vec3 Position { get; set; }
    }
}
