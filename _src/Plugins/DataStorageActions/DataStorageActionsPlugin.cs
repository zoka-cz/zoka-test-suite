using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Zoka.TestSuite.Abstraction;
using Zoka.TestSuite.DataStorageActions;

namespace Zoka.TestSuite.HttpTestActions
{
	/// <summary>
	/// 
	/// </summary>
	public class DataStorageActionsPlugin : IZokaTestSuitePlugin
	{
		/// <inheritdoc />
		public IEnumerable<CollectServicesDelegate>			CollectServicesCallback => Enumerable.Empty<CollectServicesDelegate>();
		/// <inheritdoc />
		public IEnumerable<ConfigureServicesDelegate>		ConfigureServicesCallback => new ConfigureServicesDelegate[]{ ConfiguresServices };

		private IServiceProvider							ConfiguresServices(IServiceProvider _service_provider)
		{
			_service_provider.GetRequiredService<TestPlaylistActionFactory>().RegisterPlaylistAction(DataStorageFromJsonTestAction.ACTION_TYPE_NAME, DataStorageFromJsonTestAction.ParseFromXmlElement);
			_service_provider.GetRequiredService<TestPlaylistActionFactory>().RegisterPlaylistAction(DataStorageTestAction.ACTION_TYPE_NAME, DataStorageTestAction.ParseFromXmlElement);
			_service_provider.GetRequiredService<TestPlaylistActionFactory>().RegisterPlaylistAction(StoreRegexMatchesTestAction.ACTION_TYPE_NAME, StoreRegexMatchesTestAction.ParseFromXmlElement);
			_service_provider.GetRequiredService<TestPlaylistActionFactory>().RegisterPlaylistAction(DataStorageFromJSTestAction.ACTION_TYPE_NAME, DataStorageFromJSTestAction.ParseFromXmlElement);
			return _service_provider;
		}

	}
}
