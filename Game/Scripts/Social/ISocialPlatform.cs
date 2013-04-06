using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CryEngine.Extensions;
using CryGameCode.Social.Null;
using CryEngine;

namespace CryGameCode.Social
{
	public interface ISocialPlatform
	{
		void Init(Dictionary<string, object> args);

		ISocialChat Chat { get; }
		ISocialGroup Group { get; }
	}

	public static class SocialPlatform
	{
		public static ISocialPlatform Active { get; private set; }

		public static void Init(Dictionary<string, object> args)
		{
			var options = (from type in Assembly.GetExecutingAssembly().GetTypes()
						   where type.Implements<ISocialPlatform>() && type.ContainsAttribute<ActivePlatformAttribute>()
						   select type)
						  .ToArray();

			// Single active platform: we pick that
			// No active platforms: revert to null behaviour
			// More than one, a merge has probably gone wrong, so notify via an exception
			switch (options.Length)
			{
				case 1:
					Active = (ISocialPlatform)Activator.CreateInstance(options[0]);
					break;

				case 0:
					Active = new NullPlatform();
					break;

				default:
					throw new SocialPlatformException("More than one active platform found.");
			}

			Debug.LogAlways("[Platform] Using {0}", Active.GetType().Name);
			Active.Init(args);
		}
	}

	public class ActivePlatformAttribute : Attribute
	{
	}

	public class SocialPlatformException : Exception
	{
		public SocialPlatformException()
		{
		}

		public SocialPlatformException(string message)
			: base(message)
		{
		}

		public SocialPlatformException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
