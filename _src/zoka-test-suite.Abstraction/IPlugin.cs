using System;
using System.Collections.Generic;
using Zoka.TestSuite.Abstraction;

namespace zoka_test_suite.Abstraction
{
	/// <summary>The interface, which defines any plugin into the Zoka.TestSuite</summary>
	public interface IPlugin
	{
		/// <summary>Returns the service collecting callbacks to be called during service collection. Null must not be returned, empty collection is allowed</summary>
		IEnumerable<CollectServicesDelegate> CollectServicesCallback { get; }

		/// <summary>Returns the service configuration callbacks to be called during service configuration. Null must not be returned, empty collection is allowed</summary>
		IEnumerable<ConfigureServicesDelegate> ConfigureServicesCallback { get; }
	}
}
