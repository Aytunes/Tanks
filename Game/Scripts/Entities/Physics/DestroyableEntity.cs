using CryEngine;

namespace CryGameCode.Entities.Physics
{
	[Entity(Category = "Physics", Icon = "physicsobject.bmp")]
	public class DestroyableObject : DamageableEntity
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

            InitHealth(100);
		}

        public override void OnDeath(float damage, DamageType type, Vec3 pos, Vec3 dir)
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
            breakageParams.vHitPoint = pos;

            Physics.Break(breakageParams);

            SetSlotFlags(GetSlotFlags() | EntitySlotFlags.Render);
        }

		#region Editor Properties
        [EditorProperty(Type = EditorPropertyType.Object)]
		public string Model { get { return GetObjectFilePath(); } set { LoadObject(value); } }
		#endregion
	}
}
