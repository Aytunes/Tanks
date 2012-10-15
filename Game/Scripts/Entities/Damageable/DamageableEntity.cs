using CryEngine;

namespace CryGameCode.Entities
{
	public abstract class DamageableEntity : Entity, IDamageable
	{
		public float Health { get; protected set; }
		public float MaxHealth { get; protected set; }
		public bool IsDead { get { return Health <= 0; } }

		public void Damage(float damage, DamageType type)
		{
			Health = Math.Max(Health - damage, 0);
			OnDamage(damage, type);

			if(Health <= 0 && !IsDead)
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
