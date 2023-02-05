using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zoka.TestSuite.Abstraction.XMLHelpers;

namespace Zoka.TestSuite.Abstraction
{
	/// <summary>Test playlist interface</summary>
	/// <remarks>Test Playlist is collection of playlist actions considered as single test.</remarks>
	public class TestPlaylist
	{
		#region Public data members

		/// <summary>Name of the XML Element with TestSuite</summary>
		public const string									XEL_NAME = "TestPlaylist";

		/// <summary>Name of the test - must be unique</summary>
		public string										Name { get; private set; }
		/// <summary>Description of the test</summary>
		public string										Description { get; private set; }

		/// <summary>Playlist actions</summary>
		public List<IPlaylistAction>						PlaylistActions { get; } = new List<IPlaylistAction>();

		#endregion // Public data members

		#region XML Loading

		/// <summary>Will load the Test playlist from the Xml file</summary>
		public static TestPlaylist							FromXmlFile(FileInfo _playlist_file, IServiceProvider _service_provider)
		{
			var logger = _service_provider.GetService<ILogger<TestPlaylist>>();
			logger?.LogTrace($"Loading test suite from {_playlist_file}.");
			
			XDocument x_playlist;
			using (var reader = new StreamReader(_playlist_file.OpenRead()))
				x_playlist = XDocument.Load(reader, LoadOptions.SetLineInfo);

			// check document root
			if (x_playlist.Root == null || x_playlist.Root.Name != XEL_NAME)
				throw new ZTSXmlException($"Expected root element {XEL_NAME}");

			// now we can parse the element
			return TestPlaylist.FromXmlElement(_playlist_file, x_playlist.Root, _service_provider);
		}

		/// <summary>Will load the Test playlist from the Xml element</summary>
		public static TestPlaylist							FromXmlElement(FileInfo _src_xml, XElement _element, IServiceProvider _service_provider)
		{
			var logger = _service_provider.GetService<ILogger<TestPlaylist>>();
			logger?.LogTrace($"Loading test playlist from element.");

			if (_element == null) 
				throw new ArgumentNullException(nameof(_element));

			if (_element.Name != XEL_NAME)
				throw new XmlException($"Invalid element name. Must be {XEL_NAME}, but it is {_element.Name}", null, _element.GetLineNumber(), _element.GetLinePosition());

			// is it included?
			var included_path = _element.ReadAttr<string>("_include", _src_xml, false);
			if (included_path != null)
			{
				logger?.LogTrace($"Playlist included from {included_path} -> going to read the file.");
				var included_file = new FileInfo(Path.Combine(_src_xml.DirectoryName, included_path));
				if (!included_file.Exists)
					throw new ZTSXmlException($"TestPlaylist file definition included ({included_file}) was not found.", _src_xml.Name, _element.GetLineNumber(), _element.GetLinePosition());
				return FromXmlFile(included_file, _service_provider);
			}

			var playlist = new TestPlaylist();

			playlist.Name = _element.ReadNameAttr(_src_xml, true);
			playlist.Description = _element.ReadAttr<string>("_desc", _src_xml, false);

			var action_factory = _service_provider.GetRequiredService<TestPlaylistActionFactory>();
			foreach (var x_element in _element.Elements())
			{
				var action = action_factory.LoadFromXmlElement(_src_xml, x_element, _service_provider);
				playlist.PlaylistActions.Add(action);
			}

			return playlist;
		}

		#endregion // XML Loading
	}
}
