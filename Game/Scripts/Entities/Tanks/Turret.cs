using System;

using System.Collections.Generic;
using System.Linq;

using CryEngine;
using CryEngine.Physics;
using CryEngine.Serialization;

using CryGameCode.Projectiles;

namespace CryGameCode.Tanks
{
	[Entity(Flags = EntityClassFlags.Invisible)]
	public class TurretEntity : Entity
	{
		public override void OnSpawn()
		{
			if (!Network.IsServer)
			{
				var tankName = Name.Split('.').First();

				Debug.LogAlways(tankName);
				var owner = Entity.Find(tankName) as Tank;

				owner.Turret.Initialize(this);
			}
		}
	}

	public abstract class TankTurret
	{
		public TankTurret() { }

		public TankTurret(Tank owner)
		{
			Owner = owner;

			if (Network.IsServer)
				owner.Input.OnInputChanged += OnInput;

			owner.OnDestroyed += (x) => { Destroy(); };
		}

		void OnInput(InputFlags flags, KeyEvent keyEvent)
		{
			switch (flags)
			{
				case InputFlags.LeftMouseButton:
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
					break;
				case InputFlags.RightMouseButton:
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
					break;
			}
		}

		public void Initialize(EntityBase entity)
		{
			Attachment = Owner.GetAttachment("turret");

			Attachment.SwitchToEntityObject(entity.Id);

			TurretEntity = entity;

			entity.LoadObject(Model);

			entity.Material = Material.Find("objects/tanks/tank_turrets_" + Owner.Team);

			entity.OnDestroyed += (x) => { Destroy(); };

		}

		void Serialize(CrySerialize serialize)
		{
			serialize.BeginGroup("TankTurret");

			serialize.EndGroup();
		}

		public void Destroy()
		{
			if (!TurretEntity.IsDestroyed)
				TurretEntity.Remove();

			Destroyed = true;
		}

		public void Update()
		{
			if (Destroyed || TurretEntity == null)
				return;

			if (Network.IsServer)
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
			if (TurretEntity.IsDestroyed || !Network.IsServer)
				return;

			if (Time.FrameStartTime > shotTime + (TimeBetweenShots * 1000))
			{
				shotTime = Time.FrameStartTime;

				var jointAbsolute = TurretEntity.GetJointAbsolute(helper);
				jointAbsolute.T = TurretEntity.Transform.TransformPoint(jointAbsolute.T) + jointAbsolute.Q * new Vec3(0, 0, 0);

				var gameMode = GameRules.Current as SinglePlayer;
				CryEngine.Entity.Spawn("pain", ProjectileType, jointAbsolute.T, TurretEntity.Rotation.Normalized);

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

		public abstract string Model { get; }
	}
}
