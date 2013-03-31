using CryEngine;
using CryGameCode.Entities;

using CryGameCode.Network;

namespace CryGameCode.Tanks
{
	public partial class Tank
	{
		public void OnDied(float damage, DamageType type, Vec3 pos, Vec3 dir)
		{
			if (IsLocalClient)
			{
				Debug.DrawText("Died!", 3, Color.Red, 5);

				SpawnTime = -1;

				Turret.Destroy();
				Turret = null;

				ToggleSpectatorPoint();
			}
			else
			{
				Hide(true);
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
