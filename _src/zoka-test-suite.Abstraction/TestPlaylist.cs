using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zoka.TestSuite.Abstraction.XMLHelpers;
using Zoka.ZScript;

namespace Zoka.TestSuite.Abstraction
{
	/// <summary>Test playlist interface</summary>
	/// <remarks>Test Playlist is collection of playlist actions considered as single test.</remarks>
	public class TestPlaylist
	{
		#region Private data member
		
		private readonly ILogger<TestPlaylist>				m_Logger;

		#endregion // Private data member

		#region Public data members

		/// <summary>Name of the XML Element with TestSuite</summary>
		public const string									XEL_NAME = "TestPlaylist";

		/// <summary>Name of the test - must be unique</summary>
		public string										Name { get; private set; }
		/// <summary>Description of the test</summary>
		public string										Description { get; private set; }

		/// <summary>Playlist actions</summary>
		public List<IPlaylistAction>						PlaylistActions { get; } = new List<IPlaylistAction>();

		/// <summary>Returns the names of the playlist required to be run before this playlist can be run</summary>
		public List<string>									RunAfterPlaylists { get; } = new List<string>();

		#endregion // Public data members

		#region Construction

		/// <summary>Constructor</summary>
		public TestPlaylist(ILogger<TestPlaylist> _logger)
		{
			m_Logger = _logger;
		}

		#endregion

		#region Running

		/// <summary>Will run the playlist action</summary>
		public int											Run(DataStorages _data_storages, IServiceProvider _service_provider)
		{
			foreach (var playlist_action in PlaylistActions)
			{
				var res = playlist_action.PerformAction(_data_storages, _service_provider);
				if (res != 0)
					return res;
			}

			return 0;
		}

		#endregion // Running

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

			var playlist = _service_provider.GetRequiredService<TestPlaylist>();

			playlist.Name = _element.ReadNameAttr(_src_xml, true);
			playlist.Description = _element.ReadAttr<string>("_desc", _src_xml, false);

			// run after playlists
			var run_after_el = _element.Element("RunAfter");
			if (run_after_el != null)
			{
				foreach (var playlist_el in run_after_el.Elements("Playlist"))
				{
					var name = playlist_el.ReadAttr<string>("name", _src_xml, false);
					if (name != null)
					{
						name = playlist_el.Value.Trim();
					}

					if (string.IsNullOrWhiteSpace(name))
						throw new ZTSXmlException($"RunAfter element requires Playlist elements list containing the name of the playlist to be run before this playlist. Name can be specified either as attribute name of Playlist element or as its content.", _src_xml.Name, playlist_el.GetLineNumber(), playlist_el.GetLinePosition());

					playlist.RunAfterPlaylists.Add(name);
				}
			}

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
