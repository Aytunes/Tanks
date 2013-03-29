using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CryEngine;

namespace CryGameCode
{
	public class TimeScaleModifier : IGameModifier
	{
		TimeScaleModifier() { }

		public TimeScaleModifier(float newTimeScale, float duration)
		{
			NewTimeScale = newTimeScale;

			m_timeRemaining = duration;
		}

		public void Begin()
		{
			m_prevTimeScale = TimeScale.FVal;
			TimeScale.FVal = NewTimeScale;
		}

		public bool Update()
		{
			m_timeRemaining -= Time.DeltaTime / NewTimeScale;
			if (m_timeRemaining <= 0)
			{
				TimeScale.FVal = m_prevTimeScale;

				if (OnEnd != null)
					OnEnd();
			}

			return m_timeRemaining > 0;
		}

		public event Action OnEnd;

		public float NewTimeScale { get; set; }

		CVar TimeScale { get { return CVar.Get("t_scale"); } }

		public EntityBase Target { get { return null; } set { } }

		float m_prevTimeScale;
		float m_timeRemaining;
	}
}
