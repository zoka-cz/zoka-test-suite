using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Zoka.TestSuite.Abstraction;

namespace Zoka.TestSuite.BasicTestActions
{
	/// <summary>
	/// 
	/// </summary>
	public class BasicTestsActionsPlugin : IZokaTestSuitePlugin
	{
		/// <inheritdoc />
		public IEnumerable<CollectServicesDelegate>			CollectServicesCallback => Enumerable.Empty<CollectServicesDelegate>();
		/// <inheritdoc />
		public IEnumerable<ConfigureServicesDelegate>		ConfigureServicesCallback => new ConfigureServicesDelegate[]{ ConfiguresServices };

		private IServiceProvider							ConfiguresServices(IServiceProvider _service_provider)
		{
			_service_provider.GetRequiredService<TestPlaylistActionFactory>().RegisterPlaylistAction(TestAction.ACTION_TYPE_NAME, TestAction.ParseFromXmlElement);
			return _service_provider;
		}

	}
}
