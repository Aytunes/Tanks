using CryEngine;

namespace CryGameCode.Entities.Buildings
{
	[Entity(Category = "Buildings")]
	public class Drill : DamageableEntity
	{
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

			// Physicalize
			var physicalizationParams = new PhysicalizationParams(PhysicalizationType.Rigid);

			physicalizationParams.mass = -1;
			physicalizationParams.slot = 0;

			Physicalize(physicalizationParams);

			InitHealth(100);

			Material = Material.Find("objects/tank_env_assets/scifi/drill_" + m_team);
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
