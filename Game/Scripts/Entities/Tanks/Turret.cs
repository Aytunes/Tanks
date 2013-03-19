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
		public TankTurret() { }

		public TankTurret(Tank owner)
		{
			Owner = owner;

			if (Game.IsServer)
				owner.Input.OnInputChanged += OnInput;

			owner.OnDestroyed += (x) => { Destroy(); };
		}

		void OnInput(InputFlags flags, KeyEvent keyEvent)
		{
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

			entity.Material = Material.Find("objects/tanks/tank_turrets_" + Owner.Team);

			Physicalize();

			entity.OnDestroyed += (x) => { Destroy(); };

		}

		void Physicalize()
		{
			TurretEntity.Physics.AutoUpdate = false;

			TurretEntity.Physics.Type = PhysicalizationType.Static;
			TurretEntity.Physics.Mass = Mass;

			TurretEntity.Physics.Save();
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
			
			if(!TurretEntity.IsDestroyed)
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

			var dir = tankInput.MouseWorldPosition - TurretEntity.Position;

			var ownerRotation = Owner.Rotation;
			var attachmentRotation = TurretEntity.Rotation;

			rotationZ = Quat.CreateSlerp(rotationZ, Quat.CreateRotationZ((float)Math.Atan2(-dir.X, dir.Y)), Time.DeltaTime * 10);

			attachmentRotation = rotationZ;
			attachmentRotation.Normalize();

			TurretEntity.Rotation = attachmentRotation;
		}

		Quat rotationZ;

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
				if (projectile == null || !Projectile.RecyclingEnabled)
				{
					projectile = CryEngine.Entity.Spawn("pain", ProjectileType, jointAbsolute.T, TurretEntity.Rotation.Normalized) as Projectile;
					ProjectileStorage.Add(projectile);
				}
				else
				{
					projectile.Position = jointAbsolute.T;
					projectile.Rotation = TurretEntity.Rotation.Normalized;
				}

				projectile.Launch();

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
