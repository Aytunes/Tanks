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

		void Damage(EntityId sender, float damage, DamageType type, Vec3 pos, Vec3 dir);
		void Heal(float amount);

		void InitHealth(float amount);

		event EventHandler<DamageEventArgs> OnDamaged;
		event EventHandler<DamageEventArgs> OnDeath;
	}

	public class DamageEventArgs : EventArgs
	{
		public EntityId Source { get; set; }
		public float Damage { get; set ;}
		public DamageType Type { get; set; }
		public Vec3 Position { get; set; }
		public Vec3 Direction { get; set; }
	}
}
