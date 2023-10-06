using System;
using System.Collections.Generic;
using System.Text;

namespace Zoka.TestSuite.Abstraction
{
	/// <summary>Abstraction for imported functions defined in script</summary>
	public interface IFunctionAction : IPlaylistAction
	{
		/// <summary>Will add parameter, so when function is called, the parameter may be parsed</summary>
		void AddPassedParameter(string _param_name, string _param_value_expression);
	}
}
