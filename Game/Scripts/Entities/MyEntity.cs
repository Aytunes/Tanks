using CryEngine;

namespace CryGameCode.Entities
{
    [Entity(Category = "MyCategory")]
    public class MyEntity : Entity
    {
        protected override void OnEditorReset(bool enteringGame)
        {
            LoadObject(Model);

            PlayAnimation("Default", AnimationFlags.Loop);
        }

        [EditorProperty]
        public string Model { get; set; }
    }
}
