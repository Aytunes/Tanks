using CryEngine;
using CryGameCode.Network;

namespace CryGameCode.Entities
{
	public abstract class DamageableActor : Actor, IDamageable
	{
		public void Damage(EntityId sender, float damage, DamageType type, Vec3 pos, Vec3 dir)
		{
			NetworkValidator.Server("No client-side damage");

			if (IsDead)
				return;

			RemoteDamage(sender, damage, (int)type, pos, dir);
			RemoteInvocation(RemoteDamage, NetworkTarget.ToAllClients, sender, damage, (int)type, pos, dir);

			if (IsDead)
			{
				RemoteDeath(sender, damage, (int)type, pos, dir);
				RemoteInvocation(RemoteDeath, NetworkTarget.ToAllClients, sender, damage, (int)type, pos, dir);
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
		private void RemoteDamage(EntityId sender, float damage, int type, Vec3 pos, Vec3 dir)
		{
			Health = MathHelpers.Max(Health - damage, 0);

			if (OnDamaged != null)
				OnDamaged(sender, damage, (DamageType)type, pos, dir);
		}

		[RemoteInvocation]
		private void RemoteDeath(EntityId sender, float damage, int type, Vec3 pos, Vec3 dir)
		{
			if (OnDeath != null)
				OnDeath(sender, damage, (DamageType)type, pos, dir);
		}
	
		public event OnDamagedDelegate OnDamaged;
		public event OnDamagedDelegate OnDeath;
	}
}
