using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryGameCode.Entities
{
    public interface IDamageable
	{
		float Health { get; }
		float MaxHealth { get; }
		bool IsDead { get; }

		void Damage(float damage, DamageType type);
		void Heal(float amount);

		void InitHealth(float amount);

		void OnDeath();
		void OnDamage(float damage, DamageType type);
	}
}
