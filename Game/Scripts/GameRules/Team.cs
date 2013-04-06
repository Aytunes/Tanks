using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CryEngine;

namespace CryGameCode
{
	public interface IExtendedTeamData { }

	public class Team
	{
		public Team(string name)
		{
			Name = name;
		}

		public string Name { get; private set; }

		public HashSet<Actor> Players = new HashSet<Actor>();

		/// <summary>
		/// Gets or sets extended data for the team, usually to define gamemode specific functionality per team.
		/// </summary>
		public IExtendedTeamData ExtendedData { get; set; }
	}
}
