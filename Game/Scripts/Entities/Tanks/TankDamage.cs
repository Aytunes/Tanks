using CryEngine;
using CryGameCode.Entities;

namespace CryGameCode.Tanks
{
	public partial class Tank
	{
        public override void OnDeath(float damage, DamageType type, Vec3 pos, Vec3 dir)
		{
			Debug.DrawText("Died!", 3, Color.Red, 5);

			// Don't remove tank if it was placed by hand via the Editor.
			if(Flags.HasFlag(EntityFlags.NoSave))
				Remove();
			else
				Hide(true);
		}

        public override void OnDamage(float damage, DamageType type, Vec3 pos, Vec3 dir)
		{
			Debug.DrawText(string.Format("Took {0} points of {1} damage", damage, type), 3, Color.White, 3);

			if(OnDamaged != null)
				OnDamaged(damage, type);
		}

		void Hide(bool hide)
		{
			Hidden = hide;

			if(Turret != null)
				Turret.Hide(hide);

			if(!m_leftTrack.IsDestroyed)
				m_leftTrack.Hidden = hide;

			if(!m_rightTrack.IsDestroyed)
				m_rightTrack.Hidden = hide;
		}

		public delegate void OnDamagedDelegate(float damage, DamageType type);
		public event OnDamagedDelegate OnDamaged;
	}
}
