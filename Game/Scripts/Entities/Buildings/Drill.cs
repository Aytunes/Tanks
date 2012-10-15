using CryEngine;
using CryGameCode.UI;

namespace CryGameCode.Entities.Buildings
{
	[Entity(Category = "Buildings")]
	public class Drill : DamageableEntity
	{
		private Button m_healthBar;

		protected override void OnEditorReset(bool enteringGame)
		{
			Reset(enteringGame);

			var left = team == "red";

			if(enteringGame)
			{
				m_healthBar = new Button("", 100, left ? 100 : 300, (int)Health, 20);
				OnDamage(0, DamageType.None);
			}
		}

		void Reset(bool enteringGame = true)
		{
			LoadObject(Model);
			PlayAnimation("Default", AnimationFlags.Loop);

			Physics.Type = PhysicalizationType.Rigid;
			Physics.Mass = -1;
			Physics.Slot = 0;

			Material = Material.Find("objects/tank_env_assets/scifi/drill_" + Team);

			InitHealth(100);

			if (!enteringGame && DestroyedEffect != null)
			{
				DestroyedEffect.Remove();
				DestroyedEffect = null;
			}
		}

		public override void OnDeath()
		{
			Debug.DrawText("Drill destroyed!", 3, Color.Red, 5);
			StopAnimation(blendOutTime: 1);

			DestroyedEffect = ParticleEffect.Get("smoke_and_fire.Vehicle_fires.large2");
			DestroyedEffect.Spawn(Position);
		}

		public override void OnDamage(float damage, DamageType type)
		{
			if(m_healthBar != null)
			{
				m_healthBar.Width = (int)Health;
				m_healthBar.Text = string.Format("{0}: {1}/{2}", Team, Health, MaxHealth);
			}
		}

		ParticleEffect DestroyedEffect { get; set; }

		string team = "red";
		[EditorProperty]
		public string Team 
		{
			get { return team; } 
			set 
			{
				if (string.IsNullOrEmpty(value))
					return;

				team = value; 
				Reset(); 
			}
		}

		public string Model { get { return "objects/tank_env_assets/scifi/drill.cga"; } }
	}
}
