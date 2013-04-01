using CryEngine;
using CryGameCode.Entities;

namespace CryGameCode.Tanks
{
	public partial class Tank
	{
		public void OnDied(EntityId sender, float damage, DamageType type, Vec3 pos, Vec3 dir)
		{
			var message = string.Format("{0} was killed by {1}", Name, Entity.Get(sender).Name);
			Debug.DrawText(message, 2, Color.White, 2);

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
				Metrics.Record(new Telemetry.KillData { Position = pos, DamageType = type });
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
