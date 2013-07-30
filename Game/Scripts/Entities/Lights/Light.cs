using CryEngine;

namespace CryGameCode.Entities.Lights
{
	[Entity(Category = "Lights", Icon = "Light.bmp")]
	public class Light : Entity
	{
		const int LightSlot = 1;

		public override void OnSpawn()
		{
			Flags |= EntityFlags.ClientOnly;
			OnEditorReset(true);
		}

		protected override void OnEditorReset(bool enteringGame)
		{
			Activate(Activated);
		}

		protected override void OnPropertyChanged(System.Reflection.MemberInfo memberInfo, EditorPropertyType propertyType, object newValue)
		{
			Activate(Activated, true);
		}

		public void Activate(bool activate, bool force = false)
		{
			if ((activate && !Activated) || force)
			{
				Activated = true;

				var lightParams = new LightParams();
				lightParams.lightStyle = LightStyle;
				lightParams.origin = Vec3.Zero;
				lightParams.lightFrustumAngle = 45;
				lightParams.radius = Radius;
				lightParams.coronaScale = CoronaScale;
				lightParams.coronaIntensity = CoronaIntensity;
				lightParams.coronaDistSizeFactor = CoronaDistSizeFactor;
				lightParams.coronaDistIntensityFactor = CoronaDistIntensityFactor;

				lightParams.color = DiffuseColor;
				lightParams.specularMultiplier = SpecularMultiplier;

				lightParams.hdrDynamic = HDRDynamic;

				LoadLight(lightParams, LightSlot);
			}
			else
			{
				Activated = false;
				FreeSlot(LightSlot);
			}
		}

		[EditorProperty]
		public bool Activated { get; set; }

		[EditorProperty]
		public float Radius = 10;

		#region Style
		[EditorProperty]
		public int LightStyle;

		[EditorProperty]
		public float CoronaScale = 1;
		[EditorProperty]
		public float CoronaIntensity = 1;
		[EditorProperty]
		public float CoronaDistSizeFactor = 1;
		[EditorProperty]
		public float CoronaDistIntensityFactor = 1;
		#endregion

		#region Color
		[EditorProperty(Type = EditorPropertyType.Color)]
		public Vec3 DiffuseColor { get; set; }
		[EditorProperty]
		public float DiffuseMultiplier = 1;
		[EditorProperty]
		public float SpecularMultiplier = 1;

		[EditorProperty]
		public float HDRDynamic;
		#endregion
	}
}