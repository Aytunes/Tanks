using System;
using System.Collections.Generic;
using CryEngine.Flowgraph;
using CryGameCode.Social;

namespace CryGameCode.FlowNodes
{
	[FlowNode(Category = "Social")]
	public class Login : FlowNode
	{
		[Port]
		public void Activate()
		{
			var args = new Dictionary<string, object>
			{
				{ "nickname", Environment.UserName }
			};

			SocialPlatform.Init(args);
			Done.Activate();
		}

		[Port]
		public OutputPort Done { get; set; }
	}
}
