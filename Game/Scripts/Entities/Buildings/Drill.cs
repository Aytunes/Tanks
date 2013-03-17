using CryEngine;

namespace CryGameCode.Entities.Buildings
{
	[Entity(Category = "Buildings")]
	public class Drill : DamageableEntity
	{
		private ParticleEffect m_destroyedEffect;
		private string m_team = "red";

		private const string Model = "objects/tank_env_assets/scifi/drill.cga";

		protected override void OnStartGame()
		{
			Load();
		}

		protected override void OnStartLevel()
		{
			Load();
		}

		private void Load()
		{
			LoadObject(Model);
			PlayAnimation("Default", AnimationFlags.Loop);

			Physics.Type = PhysicalizationType.Rigid;
			Physics.Mass = -1;
			Physics.Slot = 0;

			InitHealth(100);

			OnDeath += OnDied;

			Material = Material.Find("objects/tank_env_assets/scifi/drill_" + m_team);
		}

		public void OnDied(float damage, DamageType type, Vec3 pos, Vec3 dir)
		{
			if (!Game.IsServer)
				return;

			RemoteInvocation(RemoteOnDeath, NetworkTarget.ToAllClients);
		}

		[RemoteInvocation]
		private void RemoteOnDeath()
		{
			var msg = string.Format("{0}'s drill was destroyed!", Team);
			Debug.DrawText(msg, 3, Color.White, 5);

			StopAnimation(blendOutTime: 1);

			m_destroyedEffect = ParticleEffect.Get("smoke_and_fire.Vehicle_fires.large2");
			m_destroyedEffect.Spawn(Position);
		}

		[EditorProperty]
		public string Team
		{
			get { return m_team; }
			set
			{
				if (string.IsNullOrEmpty(value))
					return;

				m_team = value;
				Load();
			}
		}
	}
}
