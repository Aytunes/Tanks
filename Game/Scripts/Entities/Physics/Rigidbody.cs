using CryEngine;

namespace CryGameCode.Entities
{
	[Entity(Category = "Physics")]
	public class Rigidbody : Entity
	{
		[EditorProperty(Type = EntityPropertyType.Object)]
		public string Model { get { return GetObjectFilePath(); } set { LoadObject(value); } }

		[EditorProperty]
		public float Mass { get { return Physics.Mass; } set { Physics.Mass = value; } }

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
