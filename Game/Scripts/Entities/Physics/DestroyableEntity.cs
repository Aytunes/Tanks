using CryEngine;

namespace CryGameCode.Entities.Physics
{
	[Entity(Category = "Physics", Icon = "physicsobject.bmp")]
	public class DestroyableObject : Entity
	{
		public override void OnSpawn()
		{
			LoadObject(Model);
		}

		protected override void OnEditorReset(bool enteringGame)
		{
			SetSlotFlags(EntitySlotFlags.Render);

			LoadObject(Model);

			Physics.Mass = 5000;
			Physics.Type = PhysicalizationType.Rigid;
			Physics.Stiffness = 70;

			Destroyed = false;
		}

		protected override void  OnCollision(EntityId targetEntityId, Vec3 hitPos, Vec3 dir, short materialId, Vec3 contactNormal)
		{
			if (!Destroyed && targetEntityId!=0)
			{
				var breakageParams = new BreakageParameters();
				breakageParams.type = BreakageType.Destroy;
				breakageParams.fParticleLifeTime = 7.0f;
				breakageParams.bMaterialEffects = true;
				breakageParams.nGenericCount = 0;
				breakageParams.bForceEntity = false;
				breakageParams.bOnlyHelperPieces = false;

				breakageParams.fExplodeImpulse = 10.0f;
				breakageParams.vHitImpulse = dir;
				breakageParams.vHitPoint = hitPos;

				Physics.Break(breakageParams);

				SetSlotFlags(GetSlotFlags() | EntitySlotFlags.Render);

				Destroyed = true;
			}
		}

		#region Editor Properties
		[EditorProperty(Type = EntityPropertyType.Float, DefaultValue = 100.0f)]
		public float Health { get; set; }

		[EditorProperty(Type = EntityPropertyType.Object, DefaultValue = "objects/props/maritime/lobster_cage/lobster_cage.cgf")]
		public string Model { get { return GetObjectFilePath(); } set { LoadObject(value); } }
		#endregion

		public bool Destroyed { get; private set; }
	}
}
