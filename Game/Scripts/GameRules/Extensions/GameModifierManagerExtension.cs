using System;
using System.Collections;
using System.Collections.Generic;

using CryEngine;

using CryGameCode.Network;

namespace CryGameCode.Extensions
{
	[ServerOnly]
	public class GameModifierManagerExtension : GameRulesExtension
	{
		protected override void Init()
		{
			ReceiveUpdates = true;
		}

		public override void OnUpdate()
		{
			NetworkValidator.Server("Game modifiers are server only!");

			// Update all active modifiers,
			m_activeGameModifiers.RemoveAll(x =>
				{
					return x.Update();
				});
		}

		public IGameModifier Add<T>(params object[] args) where T : IGameModifier
		{
			NetworkValidator.Server("Game modifiers can only be added on the server");

			var type = typeof(T);

			var modifier = Activator.CreateInstance(type, args) as IGameModifier;
			m_activeGameModifiers.Add(modifier);

			//RemoteInvocation(RemoteAdd, NetworkTarget.ToRemoteClients, type.Name, args);

			return modifier;
		}

		/*[RemoteInvocation]
		void RemoteAdd(string modifierType, object[] args)
		{
			var type = Type.GetType(modifierType);

			var modifier = Activator.CreateInstance(type, args) as IGameModifier;
			m_activeGameModifiers.Add(modifier);
		}*/

		List<IGameModifier> m_activeGameModifiers = new List<IGameModifier>();
	}
}
