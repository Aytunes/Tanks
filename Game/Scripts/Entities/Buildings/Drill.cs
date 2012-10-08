using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CryEngine;

namespace CryGameCode.Entities.Buildings
{
	[Entity(Category = "Buildings")]
	public class Drill : DamageableEntity
	{
		protected override void OnReset(bool enteringGame)
		{
			Reset();
		}

		void Reset()
		{
			LoadObject(Model);
			PlayAnimation("Default", AnimationFlags.Loop);

			Physics.Type = PhysicalizationType.Rigid;
			Physics.Mass = -1;
			Physics.Slot = 0;

			Material = Material.Find("objects/tank_env_assets/scifi/drill_" + Team);

			InitHealth(100);
		}

		protected override void OnDeath()
		{
			Debug.DrawText("Drill destroyed!", 3, Color.Red, 5);
		}

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
