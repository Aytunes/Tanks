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

		public HealthModifier(IDamageable target, float healthRestoration, float restorationTime)
		{
			HealthRestoration = healthRestoration;
			RestorationTime = restorationTime;

			Target = target;
		}

		public void Begin()
		{
			m_remainingHeal = HealthRestoration;

			Target.OnDamaged += (damage, type, pos, dir) =>
			{
				// cancel heal if target took damage
				End();
			};
		}

		void End()
		{
			m_remainingHeal = 0;

			if (OnEnd != null)
				OnEnd();
		}

		public bool Update() 
		{
			var heal = HealthRestoration * Time.DeltaTime * (1 / RestorationTime);
			m_remainingHeal -= heal;

			var damageableTarget = Target as IDamageable;
			damageableTarget.Heal(heal);

			if (m_remainingHeal <= 0)
				End();

			return m_remainingHeal > 0;
		}

		public event Action OnEnd;

		public IDamageable Target { get; set; }

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
