using CryEngine;

namespace CryGameCode.Entities
{
	public abstract class DamageableEntity : Entity, IDamageable
	{
		public float Health { get; protected set; }
		public float MaxHealth { get; protected set; }
		public bool IsDead { get { return Health <= 0; } }

		public void Damage(float damage, DamageType type, Vec3 pos, Vec3 dir)
		{
			var healthAfter = Health - damage;
			var wasDead = IsDead;

			Health = MathHelpers.Max(healthAfter, 0);
			OnDamage(damage, type, pos, dir);

			if (!wasDead && healthAfter <= 0)
				OnDeath(damage, type, pos, dir);
		}

		public void Heal(float amount)
		{
			Health = MathHelpers.Min(Health + amount, MaxHealth);
		}

		public void InitHealth(float amount)
		{
			Health = MaxHealth = amount;
		}

		/// <summary>
		/// Reports entity death with info on the hit that killed it.
		/// </summary>
		/// <param name="damage"></param>
		/// <param name="type"></param>
		/// <param name="pos"></param>
		/// <param name="dir"></param>
		public virtual void OnDeath(float damage, DamageType type, Vec3 pos, Vec3 dir) { }
		public virtual void OnDamage(float damage, DamageType type, Vec3 pos, Vec3 dir) { }
	}
}
