using System;
using System.Collections.Generic;
using System.Text;

namespace Zoka.TestSuite.Abstraction
{
	/// <summary>Test playlist interface</summary>
	/// <remarks>Test Playlist is collection of playlist actions considered as single test.</remarks>
	public interface ITestPlaylist
	{
		/// <summary>Name of the test - must be unique</summary>
		string Name { get; }
	}
}
