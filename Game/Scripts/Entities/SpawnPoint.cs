using CryEngine;

using CryGameCode.Tanks;

namespace CryGameCode
{
	[Entity(Category = "Others", EditorHelper = "Objects/Tanks/tank_chassis.cgf", Icon = "", Flags = EntityClassFlags.Default)]
	public class SpawnPoint : Entity
	{
		public SpawnPoint()
		{
			LastSpawned = -1;
		}

		public bool TrySpawn(EntityBase entity)
		{
			if(entity == null)
				throw new System.ArgumentNullException("entity");

			var frameStartTime = Time.FrameStartTime;
			if((frameStartTime - LastSpawned) > SpawnDelay * 1000 || LastSpawned == -1)
			{
				LastSpawned = frameStartTime;

				entity.Position = Position;
				entity.Rotation = Rotation;

                if (entity is Tank && Team != null)
                {
                    Debug.LogAlways("Setting {0}'s team as {1}", entity.Name, Team);
                    (entity as Tank).Team = Team;
                }

				return true;
			}

			return false;
		}

		public float LastSpawned { get; private set; }

		/// <summary>
		/// Min delay between ability to spawn entities per spawnpoint.
		/// </summary>
		[EditorProperty]
		public float SpawnDelay = 3;

		[EditorProperty]
		public string Team = "red";
	}
}