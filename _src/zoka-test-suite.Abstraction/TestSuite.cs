using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Zoka.TestSuite.Abstraction.XMLHelpers;

namespace Zoka.TestSuite.Abstraction
{
	/// <summary>Class which encapsulate the whole test suite</summary>
	public class TestSuite
	{
		#region Private data members

		private readonly List<TestPlaylist>					m_Playlists = new List<TestPlaylist>();

		#endregion // Private data members

		#region Public data members

		/// <summary>Name of the XML Element with TestSuite</summary>
		public const string									XEL_NAME = "TestSuite";

		/// <summary>The name of the test suite</summary>
		public string										Name { get; private set; }

		#endregion // Public data members

		#region Construction

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

			var test_suite = new TestSuite();

			// read all playlists
			foreach (var el in x_suite.Root.Elements())
			{
				if (el.Name.LocalName != TestPlaylist.XEL_NAME)
					throw new ZTSXmlException($"Only TestPlaylist elements are allowed in the TestSuite", _test_suite_file.Name, el.GetLineNumber(), el.GetLinePosition());

				var playlist = TestPlaylist.FromXmlElement(_test_suite_file, el, _service_provider);
				test_suite.m_Playlists.Add(playlist);
			}

			logger?.LogTrace($"Loading test suite from {_test_suite_file} finished.");

			return test_suite;
		}

		#endregion // Construction
		
		/// <summary>Will run the whole test suite and returns the result of all tests</summary>
		/// <returns>
		///		0, when all tests has been successful
		///		non-zero value otherwise
		/// </returns>
		public virtual int									Run(IServiceProvider _service_provider)
		{
			return 0;
		}
		
	}
}
