using CryEngine;

namespace CryGameCode.Tanks
{
	public class Tank : Entity
	{
		public override void OnSpawn()
		{
			Input.MouseEvents += ProcessMouseEvents;

			// TODO: Allow picking tank
			LoadObject("objects/tanks/tank_laser.cdf");

			Input.RegisterAction("moveright", OnMoveRight);
			Input.RegisterAction("moveleft", OnMoveLeft);
			Input.RegisterAction("moveforward", OnMoveForward);
			Input.RegisterAction("moveback", OnMoveBack);
		}

		private void ProcessMouseEvents(MouseEventArgs e)
		{
			switch (e.MouseEvent)
			{
				// Handle turret rotation
				case MouseEvent.Move:
					break;
			}
		}

		private void OnMoveRight(ActionMapEventArgs e)
		{
			Debug.LogAlways("Moving right");

			Position += Rotation.Column0 * 1;
		}

		private void OnMoveLeft(ActionMapEventArgs e)
		{
			Debug.LogAlways("Moving left");

			Position += Rotation.Column0 * -1;
		}

		private void OnMoveForward(ActionMapEventArgs e)
		{
			Debug.LogAlways("Moving forward");

			Position += Rotation.Column1 * 1;
		}

		private void OnMoveBack(ActionMapEventArgs e)
		{
			Debug.LogAlways("Moving back");

			Position += Rotation.Column1 * -1;
		}
	}
}
