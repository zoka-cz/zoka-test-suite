using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Zoka.TestSuite.Abstraction.XMLHelpers;

namespace Zoka.TestSuite.Abstraction
{
	/// <summary>Factory to create test action</summary>
	public class TestPlaylistActionFactory
	{
		private readonly Dictionary<string, CreateFromXmlElementDelegate> m_Workers = new Dictionary<string, CreateFromXmlElementDelegate>();

		public IPlaylistAction								LoadFromXmlFile(FileInfo _action_file, IServiceProvider _service_provider)
		{

		}

		/// <summary>Will load the TestPlaylist action factory from the element</summary>
		public IPlaylistAction								LoadFromXmlElement(FileInfo _src_file, XElement _element, IServiceProvider _service_provider)
		{
			if (!m_Workers.ContainsKey(_element.Name.LocalName))
				throw new Exception($"Trying to load {_element.Name} element into IPlaylistAction, but no IPlaylistAction could be found. Didn't you forget to load appropriate plugin?");

			var included_path = _element.ReadAttr<string>("_include", _src_file.Name, false);
			if (included_path != null)
			{

			}
		}
	}
}
