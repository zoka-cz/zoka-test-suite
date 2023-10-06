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
			_service_provider.GetRequiredService<TestPlaylistActionFactory>().RegisterPlaylistAction(FunctionAction.ACTION_TYPE_NAME, FunctionAction.ParseFromXmlElement);
			_service_provider.GetRequiredService<TestPlaylistActionFactory>().RegisterPlaylistAction(LogAction.ACTION_TYPE_NAME, LogAction.ParseFromXmlElement);
			_service_provider.GetRequiredService<TestPlaylistActionFactory>().RegisterPlaylistAction(ForAction.ACTION_TYPE_NAME, ForAction.ParseFromXmlElement);
			_service_provider.GetRequiredService<TestPlaylistActionFactory>().RegisterPlaylistAction(ForEachAction.ACTION_TYPE_NAME, ForEachAction.ParseFromXmlElement);
			_service_provider.GetRequiredService<TestPlaylistActionFactory>().RegisterPlaylistAction(IfAction.ACTION_TYPE_NAME, IfAction.ParseFromXmlElement);
			_service_provider.GetRequiredService<TestPlaylistActionFactory>().RegisterPlaylistAction(BreakAction.ACTION_TYPE_NAME, BreakAction.ParseFromXmlElement);
			return _service_provider;
		}

	}
}
