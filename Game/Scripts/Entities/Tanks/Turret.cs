using System;

using System.Collections.Generic;
using System.Linq;

using CryEngine;
using CryEngine.Extensions;

using CryEngine.Physics;
using CryEngine.Serialization;

using CryGameCode.Projectiles;

namespace CryGameCode.Tanks
{
	[Entity(Flags = EntityClassFlags.Invisible)]
	public class TurretEntity : Entity
	{
		private string m_tankName;

		public override void OnSpawn()
		{
			if (!Game.IsServer)
			{
				ReceiveUpdates = true;
				m_tankName = Name.Split('.').First();
			}
		}

		public override void OnUpdate()
		{
			var owner = Entity.Find(m_tankName) as Tank;
			if (owner != null && owner.Turret != null)
			{
				owner.Turret.Initialize(this);
				ReceiveUpdates = false;
			}
		}
	}

	public abstract class TankTurret
	{
		public TankTurret(Tank owner)
		{
			Owner = owner;

			if (Game.IsServer)
				owner.Input.OnInputChanged += OnInput;

			owner.OnDeath += (s, e) => { Destroy(); };
		}

		void OnInput(InputFlags flags, KeyEvent keyEvent)
		{
			// Workaround for firing directly after the user requested revival.
			if (Owner.SpawnTime == -1 || Time.FrameStartTime - Owner.SpawnTime < 100)
				return;

			if (flags.IsSet(InputFlags.LeftMouseButton))
			{
				if (keyEvent == KeyEvent.OnPress)
				{
					if (AutomaticFire)
						m_leftFiring = true;

					ChargeWeapon();
				}
				else if (keyEvent == KeyEvent.OnRelease)
				{
					if (AutomaticFire)
						m_leftFiring = false;
					else
						FireLeft();
				}
			}

			if (flags.IsSet(InputFlags.RightMouseButton))
			{
				if (keyEvent == KeyEvent.OnPress)
				{
					if (AutomaticFire)
						m_rightFiring = true;
				}
				else if (keyEvent == KeyEvent.OnRelease)
				{
					if (AutomaticFire)
						m_rightFiring = false;
					else
						FireRight();
				}
			}
		}

		public void Initialize(EntityBase entity)
		{
			Attachment = Owner.GetAttachment("turret");

			Attachment.SwitchToEntityObject(entity.Id);

			TurretEntity = entity;

			entity.LoadObject(Model);
			entity.ViewDistanceRatio = 255;

			entity.Material = Material.Find("objects/tanks/tank_turrets_" + Owner.Team);

			Physicalize();

			entity.OnDestroyed += (x) => { Destroy(); };

		}

		void Physicalize()
		{
			var physicalizationParams = new PhysicalizationParams(PhysicalizationType.Static);

			physicalizationParams.mass = Mass;

			TurretEntity.Physicalize(physicalizationParams);
		}

		void Serialize(CrySerialize serialize)
		{
			serialize.BeginGroup("TankTurret");

			serialize.EndGroup();
		}

		public void Destroy()
		{
			if (Destroyed)
				return;

			if (TurretEntity != null && !TurretEntity.IsDestroyed)
				TurretEntity.Remove();

			foreach (var projectile in ProjectileStorage)
				projectile.Remove();

			ProjectileStorage.Clear();

			Destroyed = true;
		}

		public void Update()
		{
			if (Destroyed || TurretEntity == null)
				return;

			if (Game.IsServer)
			{
				if (m_leftFiring)
					FireLeft();

				if (m_rightFiring)
					FireRight();
			}

			var tankInput = Owner.Input;

			if (GameCVars.cam_type == (int)CameraType.FirstPerson)
			{
				var delta = m_lastMouseX - tankInput.MouseX;
				TurretEntity.Rotation *= Quat.CreateRotationZ(delta * 0.3f * (float)Math.PI / 180.0f);//TODO: take sensitivity cvar into account
				m_lastMouseX = tankInput.MouseX;
			}
			else
			{
				var dir = tankInput.MouseWorldPosition - TurretEntity.Position;

				var ownerRotation = Owner.Rotation;

				var forward = Quat.CreateRotationZ((float)Math.Atan2(-dir.X, dir.Y)).Column1;
				Vec3 up = ownerRotation.Column2;

				var rotation = new Quat(Matrix33.CreateFromVectors(forward % up, forward, up)).Normalized;
				TurretEntity.Rotation = rotation;
			}
		}

		#region Weapons
		protected virtual void ChargeWeapon() { }

		private void Fire(ref float shotTime, string helper)
		{
			if (TurretEntity.IsDestroyed || !Game.IsServer)
				return;

			if (Time.FrameStartTime > shotTime + (TimeBetweenShots * 1000))
			{
				shotTime = Time.FrameStartTime;

				var jointAbsolute = TurretEntity.GetJointAbsolute(helper);
				jointAbsolute.T = TurretEntity.Transform.TransformPoint(jointAbsolute.T) + jointAbsolute.Q * new Vec3(0, 0, 0);

				var gameMode = GameRules.Current as SinglePlayer;

				var projectile = ProjectileStorage.FirstOrDefault(x => !x.Fired);
				if (projectile != null && projectile.IsDestroyed)
				{
					ProjectileStorage.Remove(projectile);
					projectile = null;
				}

				var turretRot = TurretEntity.Rotation.Normalized;

				if (projectile == null || !Projectile.RecyclingEnabled)
				{
					projectile = CryEngine.Entity.Spawn("pain", ProjectileType, jointAbsolute.T, turretRot) as Projectile;
					ProjectileStorage.Add(projectile);
				}
				else
				{
					projectile.Position = jointAbsolute.T;
					projectile.Rotation = turretRot;
				}

				Metrics.Record(new Telemetry.WeaponFiredData { Name = ProjectileType.Name, Position = jointAbsolute.T, Rotation = turretRot.Column1 });
				projectile.Launch(Owner.Id);

				//OnFire(jointAbsolute.T);
			}
		}

		protected void FireLeft()
		{
			Fire(ref m_lastleftShot, LeftHelper);
		}

		protected void FireRight()
		{
			if (!string.IsNullOrEmpty(RightHelper))
				Fire(ref m_lastRightShot, RightHelper);
		}

		protected virtual void OnFire(Vec3 firePos) { }

		public virtual string LeftHelper { get { return "turret_term"; } }
		public virtual string RightHelper { get { return string.Empty; } }

		public virtual bool AutomaticFire { get { return false; } }
		public virtual float TimeBetweenShots { get { return 1; } }

		public virtual System.Type ProjectileType { get { return typeof(Bullet); } }

		private float m_lastleftShot;
		private float m_lastRightShot;
		private bool m_rightFiring;
		private bool m_leftFiring;

		/// <summary>
		/// Storage of projectiles that can be fired by this turret.
		/// This way we don't have to spawn new ones all the time.
		/// </summary>
		public HashSet<Projectile> ProjectileStorage = new HashSet<Projectile>();
		private int m_lastMouseX;
		#endregion

		#region Config
		public abstract string Model { get; }

		// The values below add to that of the tank, so some turrets might move slower / have a lower terminal velocity than that of other tanks.
		public virtual float Mass { get { return 0; } }
		public virtual float FrontalArea { get { return 0; } }
		public virtual float DragCoefficient { get { return 0; } }
		#endregion

		public void Hide(bool hide)
		{
			if (TurretEntity != null && !TurretEntity.IsDestroyed)
				TurretEntity.Hidden = hide;
		}

		public bool IsActive
		{
			get
			{
				return Attachment != null && !TurretEntity.IsDestroyed;
			}
		}

		public Tank Owner { get; private set; }
		public Attachment Attachment { get; private set; }
		public EntityBase TurretEntity { get; private set; }

		public bool Destroyed { get; set; }
	}
}
