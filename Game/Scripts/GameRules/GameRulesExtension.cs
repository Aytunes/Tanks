using CryEngine;
using CryEngine.Extensions;
using CryGameCode.Network;

namespace CryGameCode.Extensions
{
	public abstract class GameRulesExtension : Entity
	{
		protected SinglePlayer Rules { get; private set; }
		public bool ServerOnly
		{
			get
			{
				return GetType().ContainsAttribute<ServerOnlyAttribute>();
			}
		}

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
