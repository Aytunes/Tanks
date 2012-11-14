using CryEngine;

namespace CryGameCode.Tanks
{
	public class Chaingun : TankTurret
	{
		public Chaingun(Tank tank) : base(tank) { }
        private Chaingun() { }

        protected override void OnFire(Vec3 firePos)
        {
            /*var attachment = Attachment.GetAttachment("turret_rotation");
            if (attachment != null)
                attachment.Rotation *= Quat.CreateRotationZ(45);*/

            base.OnFire(firePos);
        }

		public override string Model { get { return "objects/tanks/turret_chaingun.chr"; } }
		public override bool AutomaticFire { get { return true; } }
		public override float TimeBetweenShots { get { return 0.05f; } }
	}
}
