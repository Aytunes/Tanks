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

			m_prevTimeScale = TimeScale.FVal;
			TimeScale.FVal = newTimeScale;
		}

		public bool Update()
		{
			m_timeRemaining -= Time.DeltaTime / NewTimeScale;
			if (m_timeRemaining <= 0)
				TimeScale.FVal = m_prevTimeScale;

			return m_timeRemaining > 0;
		}

		public float NewTimeScale { get; set; }

		CVar TimeScale { get { return CVar.Get("t_scale"); } }

		public EntityBase Target { get { return null; } set { } }

		float m_prevTimeScale;
		float m_timeRemaining;
	}
}
