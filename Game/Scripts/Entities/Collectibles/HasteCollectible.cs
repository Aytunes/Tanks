using CryEngine;

using CryGameCode.Tanks;

namespace CryGameCode.Entities.Collectibles
{
    public class HasteCollectible : Collectible
    {
        public override void OnCollected(Tank tank)
        {
            User = tank;

            User.IsBoosting = true;

            TimeRemaining = 5;
            User.SpeedMultiplier *= SpeedMultiplier;

            ReceiveUpdates = true;
        }

        public override void OnUpdate()
        {
            if (TimeRemaining <= 0 || User.IsDestroyed)
                return;

            TimeRemaining -= Time.DeltaTime;
            if (TimeRemaining <= 0)
            {
                User.SpeedMultiplier /= SpeedMultiplier;
                Remove();
            }
        }

        public override string Model
        {
            get { return "objects/tank_gameplay_assets/pickup_hologram/powerup_pickup.cga"; }
        }

        public override string TypeName
        {
            get { return "Haste"; }
        }

        public float SpeedMultiplier { get { return 2; } }

        public float TimeRemaining { get; private set; }
        public Tank User { get; private set; }
    }
}
