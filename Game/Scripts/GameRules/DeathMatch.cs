using System.Linq;

using CryEngine;

using CryGameCode.Entities;

namespace CryGameCode
{
	public class DeathMatch : SinglePlayer
	{
		public override string[] Teams
		{
			get
			{
				return new string[] { "red", "blue" };
			}
		}
	}
}
