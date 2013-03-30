using CryEngine;

namespace CryGameCode.Entities
{
	[Entity(Category = "Physics")]
	public class Rigidbody : Entity
	{
		[EditorProperty(Type = EditorPropertyType.Object)]
		public string Model { get { return GetObjectFilePath(); } set { LoadObject(value); Reset(); } }

		float mass;
		[EditorProperty]
		public float Mass
		{
			get { return mass; }
			set { mass = value; Reset(); }
		}

		public override void OnSpawn()
		{
			Reset();
		}

		protected override void OnEditorReset(bool enteringGame)
		{
			Reset();
		}

		void Reset()
		{
			var physicalizationParams = new PhysicalizationParams(PhysicalizationType.Static);

			physicalizationParams.mass = Mass;
			physicalizationParams.stiffnessScale = 70;

			Physicalize(physicalizationParams);
		}
	}
}
