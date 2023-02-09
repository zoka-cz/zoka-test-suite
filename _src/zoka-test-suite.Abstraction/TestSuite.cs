using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Zoka.TestSuite.Abstraction.XMLHelpers;
using Zoka.ZScript;

namespace Zoka.TestSuite.Abstraction
{
	/// <summary>Class which encapsulate the whole test suite</summary>
	public class TestSuite
	{
		#region Private data members

		private readonly List<TestPlaylist>					m_Playlists = new List<TestPlaylist>();
		private readonly ILogger<TestSuite>					m_Logger;
		private readonly List<(string, string)>				m_ConfigurationInitialization = new List<(string, string)> ();
		private const string								CONFIGURATION_INITIALIZATION_XEL_NAME = "DataStorageFromConfiguration";

		#endregion // Private data members

		#region Public data members

		/// <summary>Name of the XML Element with TestSuite</summary>
		public const string									XEL_NAME = "TestSuite";

		/// <summary>The name of the test suite</summary>
		public string										Name { get; private set; }

		#endregion // Public data members

		#region Construction

		/// <summary>Constructor</summary>
		public TestSuite(ILogger<TestSuite> _logger)
		{
			m_Logger = _logger;
		}

		/// <summary>Will load the test suite from the Xml structured file</summary>
		public static TestSuite								FromXml(FileInfo _test_suite_file, IServiceProvider _service_provider)
		{
			var logger = _service_provider.GetService<ILogger<TestSuite>>();
			logger?.LogTrace($"Loading test suite from {_test_suite_file}.");

			XDocument x_suite;
			using (var reader = new StreamReader(_test_suite_file.OpenRead()))
				x_suite = XDocument.Load(reader, LoadOptions.SetLineInfo);

			// check document root
			if (x_suite.Root == null || x_suite.Root.Name != XEL_NAME)
				throw new ZTSXmlException($"Expected root element {XEL_NAME}");

			var test_suite = _service_provider.GetRequiredService<TestSuite>();

			// read all playlists
			foreach (var el in x_suite.Root.Elements())
			{
				if (el.Name == CONFIGURATION_INITIALIZATION_XEL_NAME)
				{
					ReadConfigurationInitialization(test_suite, el, _test_suite_file);
					continue;
				}

				if (el.Name.LocalName != TestPlaylist.XEL_NAME)
					throw new ZTSXmlException($"Only TestPlaylist elements are allowed in the TestSuite", _test_suite_file.Name, el.GetLineNumber(), el.GetLinePosition());

				var playlist = TestPlaylist.FromXmlElement(_test_suite_file, el, _service_provider);
				test_suite.m_Playlists.Add(playlist);
			}

			logger?.LogTrace($"Loading test suite from {_test_suite_file} finished.");

			return test_suite;
		}

		private static void ReadConfigurationInitialization(TestSuite _test_suite, XElement _x_configuration_initialization, FileInfo _src_file)
		{
			if (_x_configuration_initialization == null)
				throw new ArgumentNullException(nameof(_x_configuration_initialization));
			if (_x_configuration_initialization.Name != CONFIGURATION_INITIALIZATION_XEL_NAME)
				throw new ZTSXmlException($"Expected element with name {CONFIGURATION_INITIALIZATION_XEL_NAME} for configuration initialization");

			foreach (var x_item in _x_configuration_initialization.Elements("Item"))
			{
				var from = x_item.ReadAttr<string>("from", _src_file, true);
				var into = x_item.ReadAttr<string>("into", _src_file, true);
				_test_suite.m_ConfigurationInitialization.Add((from, into));
			}
		}


		#endregion // Construction

		/// <summary>Will run the whole test suite and returns the result of all tests</summary>
		/// <returns>
		///		0, when all tests has been successful
		///		non-zero value otherwise
		/// </returns>
		public virtual int									Run(IServiceProvider _service_provider)
		{
			m_Logger?.LogInformation($"Running test suite \"{Name}\"");
			var data_storages = new DataStorages();
			data_storages.Push(new DataStorage());
			var conf = _service_provider.GetService<IConfiguration>();
			if (conf != null)
			{
				foreach (var item_to_copy in m_ConfigurationInitialization)
				{
					var section = conf.GetSection(item_to_copy.Item1);
					if (!section.Exists())
						throw new Exception($"Required configuration {item_to_copy.Item1} was not found in any configuration. Didn't you forget to include configuration file, or define that configuration?");

					data_storages.Store(item_to_copy.Item2, section.Value);
				}
			}
			foreach (var test_playlist in m_Playlists)
			{
				var res = RunPlaylist(test_playlist, data_storages, _service_provider);
				if (res != 0)
					return res;
			}

			m_Logger?.LogInformation($"All tests finished sucessfully");

			return 0;
		}

		/// <summary>Will run the playlist with its prerequisities</summary>
		protected virtual int								RunPlaylist(TestPlaylist test_playlist, DataStorages _data_storages, IServiceProvider _service_provider)
		{
			m_Logger?.LogInformation($"Running playlist \"{test_playlist}\"");
			// run prerequisities
			if (test_playlist.RunAfterPlaylists.Count > 0)
				m_Logger?.LogInformation($"Running prerequisities of playlist \"{test_playlist.Name}\"");
			foreach (var pre_playlist_name in test_playlist.RunAfterPlaylists)
			{
				var pre_playlist = m_Playlists.SingleOrDefault(p => p.Name == pre_playlist_name);
				if (pre_playlist == null)
					throw new Exception($"TestPlaylist {test_playlist} requires to run test {pre_playlist_name} before, but no such test was found.");

				m_Logger?.LogInformation($"Running prerequisity of playlist \"{test_playlist.Name}\" - playlist {pre_playlist.Name}");
				var res = RunPlaylist(pre_playlist, _data_storages, _service_provider);
				if (res != 0)
					return res;
			}
			if (test_playlist.RunAfterPlaylists.Count > 0)
				m_Logger?.LogInformation($"All prerequisities to playlist \"{test_playlist.Name}\" has been ran");

			// run playlist itself
			return test_playlist.Run(_data_storages, _service_provider);
		}
	}
}
