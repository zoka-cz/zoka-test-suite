using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Zoka.TestSuite.Abstraction
{
	/// <summary>Class which encapsulate the whole test suite</summary>
	public class TestSuite
	{
		#region Private data members

		private readonly List<ITestPlaylist>				m_Playlists = new List<ITestPlaylist>();

		#endregion // Private data members

		#region Construction

		/// <summary>Will load the test suite from the Xml structured file</summary>
		public static TestSuite								FromXml(FileInfo _test_suite_file, IServiceProvider _service_provider)
		{
			return new TestSuite();
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
