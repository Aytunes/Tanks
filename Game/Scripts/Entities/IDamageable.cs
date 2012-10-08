using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryGameCode.Entities
{
	interface IDamageable
	{
		void OnDamage(float damage, DamageType type);
	}

	public enum DamageType
	{
		None,
		Bullet,
		Explosive,
		Laser
	}
}
