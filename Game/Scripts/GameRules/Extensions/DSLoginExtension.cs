using System.Collections.Generic;
using CryGameCode.Network;
using CryGameCode.Social;

namespace CryGameCode.Extensions
{
	[DedicatedServerOnly, PriorityExtension]
	public class DSLoginExtension : GameRulesExtension
	{
		protected override void Init()
		{
			var args = new Dictionary<string, object>
			{
				{ "nickname", "Server" }
			};

			SocialPlatform.Init(args);
		}
	}
}
