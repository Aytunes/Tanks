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

                var pos = Position;
                var rot = Rotation;

				entity.Position = pos;
				entity.Rotation = rot;

                if (entity is Tank && Team != null)
                {
                    var tank = entity as Tank;
                    tank.Team = Team;

                    tank.OnRevive();

                    RemoteInvocation(NetSpawn, NetworkTarget.ToAllClients | NetworkTarget.NoLocalCalls, tank.Id, pos);
                }

				return true;
			}

			return false;
		}

        [RemoteInvocation]
        public void NetSpawn(EntityId targetId, Vec3 pos)
        {
            var tank = Entity.Get<Tank>(targetId);

            tank.Position = pos;

            tank.OnRevive();
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