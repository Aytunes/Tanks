using CryEngine;

namespace CryGameCode.Extensions
{
	public abstract class GameRulesExtension : Entity
	{
		protected SinglePlayer Rules { get; private set; }

		public void Register(SinglePlayer rules)
		{
			Debug.LogAlways("Registered new GameRules extension: {0}", GetType().Name);
			Rules = rules;

			Init();
		}

		protected virtual void Init()
		{
		}
	}
}
