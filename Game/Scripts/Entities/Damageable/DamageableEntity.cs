using CryEngine;

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

			if (OnDamaged != null)
				OnDamaged(sender, damage, type, pos, dir);

			if (!wasDead && healthAfter <= 0 && OnDeath != null)
				OnDeath(sender, damage, type, pos, dir);
		}

		public void Heal(float amount)
		{
			Health = MathHelpers.Min(Health + amount, MaxHealth);
		}

		public void InitHealth(float amount)
		{
			Health = MaxHealth = amount;
		}

		public event OnDamagedDelegate OnDamaged;
		public event OnDamagedDelegate OnDeath;
	}
}
