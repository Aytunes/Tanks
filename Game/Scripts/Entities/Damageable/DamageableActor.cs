using CryEngine;
using CryGameCode.Network;

namespace CryGameCode.Entities
{
	public abstract class DamageableActor : Actor, IDamageable
	{
		public void Damage(float damage, DamageType type, Vec3 pos, Vec3 dir)
		{
			NetworkValidator.Server("No client-side damage");

			if (IsDead)
				return;

			var healthAfter = Health - damage;

			Health = MathHelpers.Max(healthAfter, 0);

			RemoteDamage(damage, type, pos, dir);
			RemoteInvocation(RemoteDamage, NetworkTarget.ToAllClients | NetworkTarget.NoLocalCalls, damage, type, pos, dir);

			if (healthAfter <= 0)
			{
				RemoteDeath(damage, type, pos, dir);
				RemoteInvocation(RemoteDeath, NetworkTarget.ToAllClients, damage, type, pos, dir);
			}
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

		[RemoteInvocation]
		private void RemoteDamage(float damage, DamageType type, Vec3 pos, Vec3 dir)
		{
			if (OnDamaged != null)
				OnDamaged(damage, type, pos, dir);
		}

		[RemoteInvocation]
		private void RemoteDeath(float damage, DamageType type, Vec3 pos, Vec3 dir)
		{
			if (OnDeath != null)
				OnDeath(damage, type, pos, dir);
		}
	
		public event OnDamagedDelegate OnDamaged;
		public event OnDamagedDelegate OnDeath;
	}
}
