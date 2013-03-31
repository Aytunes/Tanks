using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CryEngine;

namespace CryGameCode.Entities
{
	public delegate void OnDamagedDelegate(EntityId sender, float damage, DamageType type, Vec3 pos, Vec3 dir);

	public interface IDamageable
	{
		float Health { get; }
		float MaxHealth { get; }
		bool IsDead { get; }

		void Damage(EntityId sender, float damage, DamageType type, Vec3 pos, Vec3 dir);
		void Heal(float amount);

		void InitHealth(float amount);

		event OnDamagedDelegate OnDamaged;
		event OnDamagedDelegate OnDeath;
	}
}
