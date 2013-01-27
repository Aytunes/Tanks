using CryEngine;

namespace CryGameCode.Entities
{
	[Entity(Category = "Physics")]
	public class Rigidbody : Entity
	{
		[EditorProperty(Type = EditorPropertyType.Object)]
		public string Model { get { return GetObjectFilePath(); } set { LoadObject(value); } }

        float mass;
		[EditorProperty]
        public float Mass 
        { 
            get { return mass; }
            set { Physics.Mass = mass = value; }
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
			Physics.Type = PhysicalizationType.Rigid;
		}
	}
}
