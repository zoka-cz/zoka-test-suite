using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Zoka.TestSuite.Abstraction;

namespace Zoka.TestSuite.AssertionActions
{
	/// <summary>Plugin description for assert actions</summary>
	public class AssertionActionsPlugin : IZokaTestSuitePlugin
	{
		/// <inheritdoc />
		public IEnumerable<CollectServicesDelegate>			CollectServicesCallback => Enumerable.Empty<CollectServicesDelegate>();
		/// <inheritdoc />
		public IEnumerable<ConfigureServicesDelegate>		ConfigureServicesCallback => new ConfigureServicesDelegate[] { ConfigureServices };

		
		private IServiceProvider							ConfigureServices(IServiceProvider _service_provider)
		{
			var action_factory = _service_provider.GetRequiredService<TestPlaylistActionFactory>();
			action_factory.RegisterPlaylistAction(AssertRegexMatchTestAction.ACTION_TYPE_NAME, AssertRegexMatchTestAction.ParseFromXmlElement);
			action_factory.RegisterPlaylistAction(AssertJsonObjectTestAction.ACTION_TYPE_NAME, AssertJsonObjectTestAction.ParseFromXmlElement);

			return _service_provider;
		}
	}
}
