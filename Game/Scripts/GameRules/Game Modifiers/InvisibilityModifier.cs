using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CryEngine;
using CryGameCode.Entities;

namespace CryGameCode
{
	public class InvisibilityModifier : IGameModifier
	{
		InvisibilityModifier() { }

		public InvisibilityModifier(EntityBase target, float duration)
		{
			m_timeRemaining = duration;

			Target = target;
		}

		public void Begin()
		{
			m_active = true;

			var damageableTarget = Target as IDamageable;
			damageableTarget.OnDamaged += (sender, damage, type, pos, dir) =>
			{
				// cancel heal if target took damage
				End();
			};

			var entity = Target as EntityBase;
			// TODO: Set opacity to 0 for enemies, 50 for friendlies.
			entity.Material.Opacity = 50;
		}

		void End()
		{
			var entity = Target as EntityBase;
			entity.Material.Opacity = 100;

			if (OnEnd != null)
				OnEnd();
		}

		public bool Update()
		{
			if (m_active)
			{
				m_timeRemaining -= Time.DeltaTime;
				if (m_timeRemaining <= 0)
					End();

				return m_timeRemaining > 0;
			}
			else
				return true;
		}

		public event Action OnEnd;

		public EntityBase Target { get; set; }

		float m_timeRemaining;

		public bool m_active;
	}
}
