using CryEngine;
using System;

namespace CryGameCode.Entities
{
	public abstract class DamageableEntity : Entity, IDamageable
	{
		public float Health { get; protected set; }
		public float MaxHealth { get; protected set; }
		public bool IsDead { get { return Health <= 0; } }

		public void Damage(EntityId sender, float damage, DamageType type, Vec3 pos, Vec3 dir)
		{
			var healthAfter = Health - damage;
			var wasDead = IsDead;

			Health = MathHelpers.Max(healthAfter, 0);

			var args = new DamageEventArgs { Source = sender, Damage = damage, Type = type, Position = pos, Direction = dir };

			if (OnDamaged != null)
				OnDamaged(this, args);

			if (!wasDead && healthAfter <= 0 && OnDeath != null)
				OnDeath(this, args);
		}

		public void Heal(float amount)
		{
			Health = MathHelpers.Min(Health + amount, MaxHealth);
		}

		public void InitHealth(float amount)
		{
			Health = MaxHealth = amount;
		}

		public event EventHandler<DamageEventArgs> OnDamaged;
		public event EventHandler<DamageEventArgs> OnDeath;
	}
}
