using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Zoka.TestSuite.Abstraction;

namespace Zoka.TestSuite.HttpTestActions
{
	/// <summary>
	/// 
	/// </summary>
	public class HttpTestsActionsPlugin : IZokaTestSuitePlugin
	{
		/// <inheritdoc />
		public IEnumerable<CollectServicesDelegate>			CollectServicesCallback => Enumerable.Empty<CollectServicesDelegate>();
		/// <inheritdoc />
		public IEnumerable<ConfigureServicesDelegate>		ConfigureServicesCallback => new ConfigureServicesDelegate[]{ ConfiguresServices };

		private IServiceProvider							ConfiguresServices(IServiceProvider _service_provider)
		{
			_service_provider.GetRequiredService<TestPlaylistActionFactory>().RegisterPlaylistAction(HttpRequestTestAction.ACTION_TYPE_NAME, HttpRequestTestAction.ParseFromXmlElement);
			return _service_provider;
		}

	}
}
