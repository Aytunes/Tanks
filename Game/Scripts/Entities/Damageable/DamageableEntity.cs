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
            var healthAfter = Health - damage;
            var wasDead = IsDead;

            Health = MathHelpers.Max(healthAfter, 0);
			OnDamage(damage, type);

            if (!wasDead && healthAfter <= 0)
				OnDeath();
		}

		public void Heal(float amount)
		{
            Health = MathHelpers.Min(Health + amount, MaxHealth);
		}

		public void InitHealth(float amount)
		{
			Health = MaxHealth = amount;
		}

		public virtual void OnDeath() { }
		public virtual void OnDamage(float damage, DamageType type) { }
	}
}
