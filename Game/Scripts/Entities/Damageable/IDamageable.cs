using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CryEngine;

namespace CryGameCode.Entities
{
    public interface IDamageable
	{
		float Health { get; }
		float MaxHealth { get; }
		bool IsDead { get; }

		void Damage(float damage, DamageType type, Vec3 pos, Vec3 dir);
		void Heal(float amount);

		void InitHealth(float amount);

        void OnDeath(float damage, DamageType type, Vec3 pos, Vec3 dir);
        void OnDamage(float damage, DamageType type, Vec3 pos, Vec3 dir);
	}
}
