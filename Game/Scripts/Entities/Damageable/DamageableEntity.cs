using CryEngine;

namespace CryGameCode.Entities
{
	public abstract class DamageableEntity : Entity, IDamageable
	{
		public float Health { get; protected set; }
		public float MaxHealth { get; protected set; }
		public bool IsDead { get { return Health <= 0; } }

		private bool m_diedOnce;

		public void Damage(float damage, DamageType type)
		{
            Health = MathHelpers.Max(Health - damage, 0);
			OnDamage(damage, type);

			if(IsDead && !m_diedOnce)
			{
				m_diedOnce = true;
				OnDeath();
			}
		}

		public void Heal(float amount)
		{
            Health = MathHelpers.Min(Health + amount, MaxHealth);
		}

		public void InitHealth(float amount)
		{
			m_diedOnce = false;
			Health = amount;
			MaxHealth = amount;
		}

		public virtual void OnDeath() { }
		public virtual void OnDamage(float damage, DamageType type) { }
	}
}
