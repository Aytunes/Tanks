using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CryEngine;

namespace CryGameCode
{
	public interface IGameModifier
	{
		/// <summary>
		/// Begin the game modifier.
		/// </summary>
		void Begin();

		/// <summary>
		/// Called each frame when the modifier is active.
		/// </summary>
		/// <returns>false if the game modifier is done, and should be removed</returns>
		bool Update();

        EntityBase Target { get; set; }

		event Action OnEnd;
	}
}
