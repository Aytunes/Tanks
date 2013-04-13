using CryEngine;
using CryGameCode.Entities;

namespace CryGameCode.Tanks
{
	public partial class Tank
	{
		public void OnDied(object sender, DamageEventArgs e)
		{
			if (IsLocalClient)
			{
				SpawnTime = -1;

				Turret.Destroy();
				Turret = null;

				ToggleSpectatorPoint();
			}
			else
			{
				Hide(true);
			}

			if (Game.IsServer)
			{
				Metrics.Record(new Telemetry.KillData { Position = e.Position.ToIntVec2(), DamageType = e.Type });
			}
		}

		void Hide(bool hide)
		{
			Hidden = hide;

			if (Turret != null)
				Turret.Hide(hide);
		}

		float m_health;
		public override float Health
		{
			get
			{
				return m_health;
			}
			set
			{
				m_health = value;
			}
		}

		public override float MaxHealth
		{
			get
			{
				return 100;
			}
		}
	}
}
