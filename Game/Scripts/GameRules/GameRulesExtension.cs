using System;
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

		public bool DSOnly
		{
			get
			{
				return GetType().ContainsAttribute<DedicatedServerOnlyAttribute>();
			}
		}

		public bool Active
		{
			get
			{
				return (!ServerOnly || Game.IsServer) && (!DSOnly || Game.IsDedicated);
			}
		}

		public void Register(SinglePlayer rules)
		{
			if (!Active)
				return;

			Debug.LogAlways("Registered new GameRules extension: {0}", GetType().Name);
			Rules = rules;

			Init();
		}

		protected override bool OnRemove()
		{
			if (Active)
				Destroy();

			return true;
		}

		protected virtual void Init()
		{
		}

		protected virtual void Destroy()
		{
		}
	}

	public class PriorityExtensionAttribute : Attribute
	{
	}
}
