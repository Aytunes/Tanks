using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CryEngine;
using CryGameCode.Entities;

namespace CryGameCode
{
	public class HealthModifier : IGameModifier
	{
		HealthModifier() { }

		public HealthModifier(EntityId targetId, float healthRestoration, float restorationTime)
		{
			HealthRestoration = healthRestoration;
			RestorationTime = restorationTime;

			Target = Entity.Get(targetId);

			m_remainingHeal = healthRestoration;

			var damageableTarget = Target as IDamageable;
			damageableTarget.OnDamaged += (sender, e) =>
			{
				// cancel heal if target took damage
				m_remainingHeal = 0;
			};
		}

		public bool Update()
		{
			var heal = HealthRestoration * Time.DeltaTime * (1 / RestorationTime);
			m_remainingHeal -= heal;

			var damageableTarget = Target as IDamageable;
			damageableTarget.Heal(heal);

			return m_remainingHeal > 0;
		}

		public EntityBase Target { get; set; }

		/// <summary>
		/// Amount of health restored.
		/// </summary>
		public float HealthRestoration { get; set; }

		/// <summary>
		/// Time in seconds to restore health, aborted it entity is attacked.
		/// </summary>
		public float RestorationTime { get; set; }

		float m_remainingHeal;
	}
}
