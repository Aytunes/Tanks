using CryEngine;
using CryGameCode.Entities;

namespace CryGameCode.Tanks
{
	public partial class Tank
	{
		public void OnDied(float damage, DamageType type, Vec3 pos, Vec3 dir)
		{
			Debug.DrawText("Died!", 3, Color.Red, 5);

			SpawnTime = -1;

			Turret.Destroy();
			Turret = null;

			ToggleSpectatorPoint();
		}

		void Hide(bool hide)
		{
			Hidden = hide;

			if (Turret != null)
				Turret.Hide(hide);
		}
	}
}
