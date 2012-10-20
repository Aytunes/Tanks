using CryEngine;

namespace CryGameCode.Entities
{
	public abstract class DamageableActor : Actor, IDamageable
	{
		public void Damage(float damage, DamageType type)
		{
			Health = Math.Max(Health - damage, 0);
			OnDamage(damage, type);

			if (Health <= 0 && !IsDead)
				OnDeath();
		}

		public void Heal(float amount)
		{
			Health = Math.Min(Health + amount, MaxHealth);
		}

		public void InitHealth(float amount)
		{
			Health = amount;
			MaxHealth = amount;
		}

		public virtual void OnDeath() { }
		public virtual void OnDamage(float damage, DamageType type) { }
	}
}
