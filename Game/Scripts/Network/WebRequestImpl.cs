using System.IO;
using System.Xml.Linq;
using CryEngine;

namespace CryGameCode.Network
{
	[Entity(Flags = EntityClassFlags.Invisible)]
	public class StringWebRequestEntity : WebRequestEntity<string>
	{
		protected override string Convert(string data)
		{
			return data;
		}
	}

	[Entity(Flags = EntityClassFlags.Invisible)]
	public class XmlWebRequestEntity : WebRequestEntity<XDocument>
	{
		protected override XDocument Convert(string data)
		{
			return XDocument.Load(new StringReader(data));
		}
	}
}
