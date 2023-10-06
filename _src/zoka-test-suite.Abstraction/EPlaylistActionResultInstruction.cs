using System;
using System.Collections.Generic;
using System.Text;

namespace Zoka.TestSuite.Abstraction
{
	/// <summary>List of results returned from performing Playlist action, which contains some instruction for further processing</summary>
	public enum EPlaylistActionResultInstruction
	{
		/// <summary>Processing should continue as planned</summary>
		NoInstruction,
		/// <summary>Break the current loop (equivalent to the break command)</summary>
		BreakLoop,
		/// <summary>Continue with the next loop (equivalent to the continue command)</summary>
		ContinueLoop,
		/// <summary>Should fail further processing</summary>
		Fail,

	}
}
