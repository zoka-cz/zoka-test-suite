using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Zoka.TestSuite.Abstraction.XMLHelpers;

namespace Zoka.TestSuite.Abstraction
{
	/// <summary>Factory to create test action</summary>
	public class TestPlaylistActionFactory
	{
		private readonly Dictionary<string, CreateFromXmlElementDelegate> m_Workers = new Dictionary<string, CreateFromXmlElementDelegate>();

		/// <summary>Will register the playlist action</summary>
		public void											RegisterPlaylistAction(string _name, CreateFromXmlElementDelegate _action)
		{
			if (m_Workers.ContainsKey(_name))
				throw new ArgumentException($"TestPlaylist action with name \"{_name}\" is already registered.", nameof(_name));

			m_Workers.Add(_name, _action);
		}

		/// <summary>Will load the IPlaylistAction from the XML file</summary>
		public IPlaylistAction								LoadFromXmlFile(FileInfo _action_file, List<IFunctionAction> _imported_functions, IServiceProvider _service_provider)
		{
			var logger = _service_provider.GetService<ILogger<TestPlaylistActionFactory>>();
			logger?.LogTrace($"Loading test playlist action from {_action_file}.");

			XDocument x_action;
			using (var reader = new StreamReader(_action_file.OpenRead()))
				x_action = XDocument.Load(reader, LoadOptions.SetLineInfo);

			// now we can parse the element
			return LoadFromXmlElement(_action_file, x_action.Root, _imported_functions, _service_provider);
		}

		/// <summary>Will load the TestPlaylist action factory from the XML element</summary>
		public IPlaylistAction								LoadFromXmlElement(FileInfo _src_file, XElement _element, List<IFunctionAction> _imported_functions, IServiceProvider _service_provider)
		{
			var logger = _service_provider.GetService<ILogger<TestPlaylistActionFactory>>();
			logger?.LogTrace($"Loading test playlist action from element {_element.Name}.");
			
			if (!m_Workers.ContainsKey(_element.Name.LocalName))
			{
				var imported_func = _imported_functions.FirstOrDefault(f => f.Name == _element.Name.LocalName);
				if (imported_func != null)
				{
					foreach (var x_attribute in _element.Attributes())
					{
						imported_func.AddPassedParameter(x_attribute.Name.LocalName, x_attribute.Value);
					}
					return imported_func;
				}
				if (_imported_functions.Any(f => f.Name == _element.Name.LocalName))
					throw new Exception($"Trying to load {_element.Name} element into IPlaylistAction, but no IPlaylistAction could be found. Didn't you forget to load appropriate plugin?");
			}

			var included_path = _element.ReadAttr<string>("_include", _src_file, false);
			if (included_path != null)
			{
				logger?.LogTrace($"Playlist action included from {included_path} -> going to read the file.");
				var included_file = new FileInfo(Path.Combine(_src_file.DirectoryName!, included_path));
				if (!included_file.Exists)
					throw new ZTSXmlException($"PlaylistAction file definition included ({included_file}) was not found.", _src_file.Name, _element.GetLineNumber(), _element.GetLinePosition());
				return LoadFromXmlFile(included_file, _imported_functions, _service_provider);
			}

			var playlist_action = m_Workers[_element.Name.LocalName](_src_file, _element, _imported_functions, _service_provider);
			return playlist_action;
		}
	}
}
