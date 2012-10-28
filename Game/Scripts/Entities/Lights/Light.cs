using CryEngine;

namespace CryGameCode.Entities.Lights
{
	[Entity(Category = "Lights", Icon="Light.bmp")]
	public class Light : Entity
	{
		const int LightSlot = 1;

		protected override void OnInit()
		{
			Flags |= EntityFlags.ClientOnly;
			OnEditorReset(true);
		}

		protected override void OnEditorReset(bool enteringGame)
		{
			Activate(Activated);
		}

		protected override void OnPropertyChanged(System.Reflection.MemberInfo memberInfo, EntityPropertyType propertyType, object newValue)
		{
			Activate(Activated, true);
		}

		public void Activate(bool activate, bool force = false)
		{
			if((activate && !Activated) || force)
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

		[EditorProperty(DefaultValue = 10)]
		public float Radius;

		#region Style
		[EditorProperty]
		public int LightStyle;

		[EditorProperty(DefaultValue = 1)]
		public float CoronaScale;
		[EditorProperty(DefaultValue = 1)]
		public float CoronaIntensity;
		[EditorProperty(DefaultValue = 1)]
		public float CoronaDistSizeFactor;
		[EditorProperty(DefaultValue = 1)]
		public float CoronaDistIntensityFactor;
		#endregion

		#region Color
		[EditorProperty(Type = EntityPropertyType.Color)]
		public Vec3 DiffuseColor { get; set; }
		[EditorProperty(DefaultValue = 1)]
		public float DiffuseMultiplier;
		[EditorProperty(DefaultValue = 1)]
		public float SpecularMultiplier;

		[EditorProperty]
		public float HDRDynamic;
		#endregion
	}
}