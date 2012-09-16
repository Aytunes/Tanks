using CryEngine;

namespace CryGameCode
{
	[Entity(Category = "Others", EditorHelper = "Objects/Tanks/tank_chassis.cgf", Icon = "", Flags = EntityClassFlags.Default)]
	public class SpawnPoint : Entity
	{
		static SpawnPoint()
		{
			CVar.RegisterFloat("g_spawnPointUsageDelay", ref SpawnDelay);
		}

		public SpawnPoint()
		{
			LastSpawned = -1;
		}

		public bool TrySpawn(EntityBase entity)
		{
			if (entity == null)
				throw new System.ArgumentNullException("entity");

			var frameStartTime = Time.FrameStartTime;
			if ((frameStartTime - LastSpawned) > SpawnDelay * 1000 || LastSpawned == -1)
			{
				LastSpawned = frameStartTime;

				entity.Position = Position;
				entity.Rotation = Rotation;

				Debug.LogAlways("Spawned entity {0} with id {1} at {2},{3},{4}", entity.Name, entity.Id, Position.X, Position.Y, Position.Z);
				return true;
			}

			return false;
		}

		public float LastSpawned { get; private set; }

		/// <summary>
		/// Min delay between ability to spawn entities per spawnpoint.
		/// </summary>
		static float SpawnDelay = 3;
	}
}