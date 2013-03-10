using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CryEngine;
using CryEngine.Extensions;
using CryEngine.Serialization;

namespace CryGameCode.Tanks
{
	public class PlayerInput : IPlayerInput
	{
		// Default constructor for realtime scripting 
		PlayerInput() { }

		public PlayerInput(Tank tank)
		{
			Owner = tank;

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

			Input.ActionmapEvents.Add("attack1", (e) =>
			{
				if (e.KeyEvent == KeyEvent.OnRelease)
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

						if (Network.IsServer)
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
			});

			Input.ActionmapEvents.Add("cycle_view", (e) =>
			{
				if (GameCVars.cam_type < (int)CameraType.Last - 1)
					GameCVars.cam_type++;
				else
					GameCVars.cam_type = 0;
			});

			Input.MouseEvents += (mouseArgs =>
			{
				if (Owner == null || Owner.IsDestroyed)
					return;

				m_mousePositionX = mouseArgs.X;
				m_mousePositionY = mouseArgs.Y;

				Owner.GameObject.NotifyNetworkStateChange(Aspect);
			});
		}

		public void PreUpdate() { }
		public void Update() { }
		public void PostUpdate() { }

		public void NetSerialize(CryEngine.Serialization.CrySerialize serialize)
		{
			serialize.BeginGroup("PlayerInput");

			var flags = (uint)m_flags;
			serialize.Value("m_flags", ref flags);
			m_flags = (InputFlags)flags;

			serialize.Value("m_mousePositionX", ref m_mousePositionX);
			serialize.Value("m_mousePositionY", ref m_mousePositionY);

			serialize.EndGroup();
		}

		void AddEvent(string actionMapName, InputFlags flag)
		{
			Input.ActionmapEvents.Add(actionMapName, (e) => { SetFlag(flag, e.KeyEvent); });
		}

		public void Destroy()
		{
			if (Owner.IsLocalClient)
				Input.ActionmapEvents.RemoveAll(this);
		}

		void SetFlag(InputFlags flag, KeyEvent keyEvent)
		{
			var hasFlag = HasFlag(flag);

			switch (keyEvent)
			{
				case KeyEvent.OnPress:
					{
						if (!hasFlag)
						{
							Flags |= flag;

							Owner.GameObject.NotifyNetworkStateChange(Aspect);
						}
					}
					break;
				case KeyEvent.OnRelease:
					{
						if (hasFlag)
						{
							Flags &= ~flag;

							Owner.GameObject.NotifyNetworkStateChange(Aspect);
						}
					}
					break;
			}
		}

		public bool HasFlag(InputFlags flag)
		{
			// Enum.HasFlag is very slow, avoid usage.
			return ((Flags & flag) == flag);
		}

		public static int Aspect = 256;

		InputFlags m_flags;
		public InputFlags Flags { get { return m_flags; } set { m_flags = value; } }

		int m_mousePositionX;
		public int MouseX { get { return m_mousePositionX; } }
		int m_mousePositionY;
		public int MouseY { get { return m_mousePositionY; } }

		public Actor Owner { get; private set; }
	}
}
