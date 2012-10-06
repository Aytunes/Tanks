using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CryEngine;

namespace CryGameCode.Projectiles
{
	public class Rocket : Entity
	{
		static Rocket()
		{
			CVar.RegisterFloat("g_tankRocketSpeed", ref rocketSpeed);
		}

		public override void OnSpawn()
		{
			LoadObject(Model);

			Physics.Type = PhysicalizationType.Rigid;
			Physics.Mass = Mass;

			TravelDir = Rotation.Column1;
			Velocity = TravelDir * rocketSpeed;
		}

		protected override void OnCollision(EntityId targetEntityId, Vec3 hitPos, Vec3 dir, short materialId, Vec3 contactNormal)
		{
			var effect = ParticleEffect.Get("explosions.explosive_bullet.default");
			effect.Spawn(hitPos);

			Remove();
		}

		public static float rocketSpeed = 1290;

		public Vec3 TravelDir { get; private set; }

		public string Model = "objects/projectiles/rocket.cgf";
		public float Mass = 20;
	}
}
