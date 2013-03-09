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

			if (Owner.IsLocalClient)
			{
				// Temp hax for right mouse events not working
				Input.ActionmapEvents.Add("attack2", (e) =>
				{
					switch (e.KeyEvent)
					{
						case KeyEvent.OnPress:
							if (AutomaticFire)
								m_rightFiring = true;
							break;

						case KeyEvent.OnRelease:
							if (AutomaticFire)
								m_rightFiring = false;
							else
								FireRight();
							break;
					}
				});

				Input.MouseEvents += ProcessMouseEvents;
			}

			owner.OnDestroyed += (x) => { Destroy(); };
		}

		public void Initialize(EntityBase entity)
		{
			Attachment = Owner.GetAttachment("turret");

			Attachment.SwitchToEntityObject(entity.Id);

			Entity = entity;

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
			if (Owner.IsLocalClient)
			{
				Input.MouseEvents -= ProcessMouseEvents;
				Input.ActionmapEvents.RemoveAll(this);
			}

			if (!Entity.IsDestroyed)
				Entity.Remove();

			Destroyed = true;
		}

		private void ProcessMouseEvents(MouseEventArgs e)
		{
			switch (e.MouseEvent)
			{
				case MouseEvent.LeftButtonDown:
					{
						if (AutomaticFire)
							m_leftFiring = true;

						ChargeWeapon();
					}
					break;

				case MouseEvent.LeftButtonUp:
					{
						if (AutomaticFire)
							m_leftFiring = false;
						else
							FireLeft();
					}
					break;
			}
		}

		public void Update()
		{
			if (Destroyed)
				return;

			if (m_leftFiring)
				FireLeft();

			if (m_rightFiring)
				FireRight();

			var tankInput = Owner.Input;

			var dir = Renderer.ScreenToWorld(tankInput.MouseX, tankInput.MouseY) - Entity.Position;

			var ownerRotation = Owner.Rotation;
			var attachmentRotation = Entity.Rotation;

			rotationZ = Quat.CreateSlerp(rotationZ, Quat.CreateRotationZ((float)Math.Atan2(-dir.X, dir.Y)), Time.DeltaTime * 10);

			attachmentRotation = rotationZ;
			attachmentRotation.Normalize();

			Entity.Rotation = attachmentRotation;
		}

		Quat rotationZ;

		#region Weapons
		protected virtual void ChargeWeapon() { }

		private void Fire(ref float shotTime, string helper)
		{
			if (Entity.IsDestroyed)
				return;

			if (Time.FrameStartTime > shotTime + (TimeBetweenShots * 1000))
			{
				shotTime = Time.FrameStartTime;

				var jointAbsolute = Entity.GetJointAbsolute(helper);
				jointAbsolute.T = Entity.Transform.TransformPoint(jointAbsolute.T) + jointAbsolute.Q * new Vec3(0, 0, 0);

				var gameMode = GameRules.Current as SinglePlayer;
				Owner.RemoteInvocation(gameMode.RequestEntitySpawn, NetworkTarget.ToServer, ProjectileType.FullName, jointAbsolute.T, Entity.Rotation.Normalized);

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
			if (Entity != null && !Entity.IsDestroyed)
				Entity.Hidden = hide;
		}

		public bool IsActive
		{
			get
			{
				return Attachment != null && !Entity.IsDestroyed;
			}
		}

		public Tank Owner { get; private set; }
		public Attachment Attachment { get; private set; }
		public EntityBase Entity { get; private set; }

		public bool Destroyed { get; set; }

		public abstract string Model { get; }
	}
}
