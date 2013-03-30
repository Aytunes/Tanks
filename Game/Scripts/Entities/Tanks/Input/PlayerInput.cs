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

			Input.ActionmapEvents.Add("cycle_view", (e) =>
			{
				if (GameCVars.cam_type < (int)CameraType.Last - 1)
					GameCVars.cam_type++;
				else
					GameCVars.cam_type = 0;
			});

			if (Game.IsClient)
				Input.MouseEvents += OnMouseEvent;
		}

		public void OnMouseEvent(MouseEventArgs e)
		{
			if (e.MouseEvent == MouseEvent.Move)
				UpdatePos(e.X, e.Y);
		}

		private void UpdatePos(int x, int y)
		{
			m_mousePositionX = x;
			m_mousePositionY = y;

			m_mouseWorldPos = Renderer.ScreenToWorld(m_mousePositionX, m_mousePositionY);

			if (Owner != null && Owner.GameObject != null)
				Owner.GameObject.NotifyNetworkStateChange(Aspect);
		}

		public void Update()
		{
			if (!Owner.IsLocalClient)
				return;

			if (HasAnyFlag(InputFlags.MoveBack, InputFlags.MoveForward, InputFlags.MoveLeft, InputFlags.MoveRight))
				UpdatePos(Input.MouseX, Input.MouseY);
		}

		public void PreUpdate() { }

		public void PostUpdate() { }

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

		public bool HasAnyFlag(params InputFlags[] targets)
		{
			foreach (var target in targets)
			{
				if (HasFlag(target))
					return true;
			}

			return false;
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
