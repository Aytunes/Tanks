using CryEngine;

namespace CryGameCode.Entities
{
	public abstract class DamageableActor : Actor, IDamageable
	{
		public void Damage(float damage, DamageType type, Vec3 pos, Vec3 dir)
		{
			var healthAfter = Health - damage;
			var wasDead = IsDead;

			Health = MathHelpers.Max(healthAfter, 0);

			if (OnDamaged != null)
				OnDamaged(damage, type, pos, dir);

			if (!wasDead && healthAfter <= 0 && OnDeath != null)
				OnDeath(damage, type, pos, dir);
		}

		public void Heal(float amount)
		{
			Health = MathHelpers.Min(Health + amount, MaxHealth);
		}

		public void InitHealth(float amount)
		{
			Health = amount;
			MaxHealth = amount;
		}

		public event OnDamagedDelegate OnDamaged;
		public event OnDamagedDelegate OnDeath;
	}
}
