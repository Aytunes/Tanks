using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CryEngine;
using CryEngine.Serialization;

namespace CryGameCode.Tanks
{
	public class PlayerInput
	{
		public PlayerInput(Tank tank)
		{
			Owner = tank;
			m_mouseWorldPos = Vec3.Zero;

			Owner.OnDestroyed += (x) => { Destroy(); };
		}

		public void RegisterInputs()
		{
			AddEvent("zoom_in", InputFlags.ZoomIn);
			AddEvent("zoom_out", InputFlags.ZoomOut);

			AddEvent("moveright", InputFlags.MoveRight);
			AddEvent("moveleft", InputFlags.MoveLeft);

			AddEvent("moveforward", InputFlags.MoveForward);
			AddEvent("moveback", InputFlags.MoveBack);

			AddEvent("sprint", InputFlags.Boost);

			AddEvent("attack1", InputFlags.LeftMouseButton);
			AddEvent("attack2", InputFlags.RightMouseButton);

			OnInputChanged += (flags, keyEvent) =>
			{
				if (flags.IsSet(InputFlags.LeftMouseButton) && keyEvent == KeyEvent.OnRelease)
				{
					var gameRules = GameRules.Current as SinglePlayer;

					if (Owner != null && Owner.IsDead && !Owner.IsDestroyed)
					{
						var tank = Owner as Tank;

						// Set team &  type, sent to server and remote clients on revival. (TODO: Allow picking via UI)
						tank.Team = gameRules.Teams.ElementAt(SinglePlayer.Selector.Next(0, gameRules.Teams.Length));

						if (string.IsNullOrEmpty(GameCVars.ForceTankType))
							tank.TurretTypeName = GameCVars.TurretTypes[SinglePlayer.Selector.Next(GameCVars.TurretTypes.Count)].FullName;
						else
							tank.TurretTypeName = "CryGameCode.Tanks." + GameCVars.ForceTankType;

						if (Game.IsServer)
							gameRules.RequestRevive(tank.Id, tank.Team, tank.TurretTypeName);
						else
						{
							Debug.LogAlways("Requesting revive ({0}, {1}, {2})", tank.Id, tank.Team, tank.TurretTypeName);
							Owner.RemoteInvocation(gameRules.RequestRevive, NetworkTarget.ToServer, tank.Id, tank.Team, tank.TurretTypeName);
						}
					}
					else if (Owner == null)
						Debug.LogAlways("Could not request revive, owner as null");
					else if (Owner.IsDead)
						Debug.LogAlways("Could not request revive, owner was alive.");
				}
			};

			Input.ActionmapEvents.Add("cycle_view", (e) =>
			{
				if (GameCVars.cam_type < (int)CameraType.Last - 1)
					GameCVars.cam_type++;
				else
					GameCVars.cam_type = 0;
			});

            Input.MouseEvents += OnMouseEvent;
		}

		public void PreUpdate() { }

		public void PostUpdate() { }

        public void OnMouseEvent(MouseEventArgs e)
        {
			if (e.MouseEvent == MouseEvent.Move)
            {
                m_mousePositionX = e.X;
                m_mousePositionY = e.Y;

                m_mouseWorldPos = Renderer.ScreenToWorld(m_mousePositionX, m_mousePositionY);

                Owner.GameObject.NotifyNetworkStateChange(Aspect);
            }
        }

		public void NetSerialize(CryEngine.Serialization.CrySerialize serialize)
		{
			serialize.BeginGroup("PlayerInput");

			var flags = (uint)m_flags;
			serialize.EnumValue("m_flags", ref flags, (uint)InputFlags.First, (uint)InputFlags.Last);

			if (Game.IsServer && OnInputChanged != null)
			{
				var changedKeys = (InputFlags)flags ^ m_flags;

				var pressedKeys = changedKeys & (InputFlags)flags;
                if (pressedKeys != 0)
                    OnInputChanged(pressedKeys, KeyEvent.OnPress);

				var releasedKeys = changedKeys & m_flags;
                if (releasedKeys != 0)
                    OnInputChanged(releasedKeys, KeyEvent.OnRelease);
			}

			m_flags = (InputFlags)flags;

			serialize.Value("m_mousePositionX", ref m_mousePositionX);
			serialize.Value("m_mousePositionY", ref m_mousePositionY);

			serialize.Value("m_mouseWorldPos", ref m_mouseWorldPos, "wrld");

			serialize.EndGroup();
		}

		public delegate void OnInputChangedDelegate(InputFlags flags, KeyEvent keyEvent);
		public event OnInputChangedDelegate OnInputChanged;

		void AddEvent(string actionMapName, InputFlags flag)
		{
			Input.ActionmapEvents.Add(actionMapName, (e) => { SetFlag(flag, e.KeyEvent); });
		}

		public void Destroy()
		{
			if (Owner.IsLocalClient)
			{
				Input.MouseEvents -= OnMouseEvent;
				Input.ActionmapEvents.RemoveAll(this);
			}
		}

		void SetFlag(InputFlags flag, KeyEvent keyEvent)
		{
			if (Owner == null || Owner.IsDestroyed)
				return;

			var hasFlag = HasFlag(flag);

			switch (keyEvent)
			{
				case KeyEvent.OnPress:
					{
						if (!hasFlag)
						{
							Flags |= flag;

							Owner.GameObject.NotifyNetworkStateChange(Aspect);

                            if (OnInputChanged != null && (Game.IsPureClient || !Game.IsMultiplayer))
								OnInputChanged(flag, keyEvent);
						}
					}
					break;
				case KeyEvent.OnRelease:
					{
						if (hasFlag)
						{
							Flags &= ~flag;

							Owner.GameObject.NotifyNetworkStateChange(Aspect);

                            if (OnInputChanged != null && (Game.IsPureClient || !Game.IsMultiplayer))
								OnInputChanged(flag, keyEvent);
						}
					}
					break;
			}
		}

		public bool HasFlag(InputFlags target)
		{
			return Flags.IsSet(target);
		}

		public static int Aspect = 256;

		InputFlags m_flags;
		public InputFlags Flags { get { return m_flags; } set { m_flags = value; } }

		int m_mousePositionX;
		public int MouseX { get { return m_mousePositionX; } }
		int m_mousePositionY;
		public int MouseY { get { return m_mousePositionY; } }

		Vec3 m_mouseWorldPos;
		public Vec3 MouseWorldPosition { get { return m_mouseWorldPos; } }

		public Actor Owner { get; private set; }
	}
}
