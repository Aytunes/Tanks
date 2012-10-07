using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CryEngine;

namespace CryGameCode.Entities.Buildings
{
	[Entity(Category = "Buildings")]
	public class Drill : Entity
	{
		public override void OnSpawn()
		{
			Reset();
		}

		void Reset()
		{
			LoadObject(Model);
			PlayAnimation("Default", AnimationFlags.Loop);

			Physics.Type = PhysicalizationType.Rigid;
			Physics.Mass = -1;

			Material = Material.Find("objects/tank_env_assets/scifi/drill_" + Team);
		}

		protected override void OnCollision(EntityId targetEntityId, Vec3 hitPos, Vec3 dir, short materialId, Vec3 contactNormal)
		{
			if (targetEntityId != 0 && targetEntityId != Id)
			{
				Health -= DamagePerCollision;

				if (Health <= 0)
					Debug.DrawText(Team.ToUpper() + "'S DRILL HAS BEEN DESTROYED", 3.0f, Color.Green, 3.0f);
			}
		}

		[EditorProperty]
		public float Health = 100;

		[EditorProperty]
		public float DamagePerCollision = 20;

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
