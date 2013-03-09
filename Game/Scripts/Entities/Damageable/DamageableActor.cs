using CryEngine;

namespace CryGameCode.Entities
{
	public abstract class DamageableActor : Actor, IDamageable
	{
		public void Damage(float damage, DamageType type, Vec3 pos, Vec3 dir)
		{
			Health = MathHelpers.Max(Health - damage, 0);
			OnDamage(damage, type, pos, dir);

			if (Health <= 0 && !IsDead)
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

		public virtual void OnDeath(float damage, DamageType type, Vec3 pos, Vec3 dir) { }
		public virtual void OnDamage(float damage, DamageType type, Vec3 pos, Vec3 dir) { }
	}
}
