using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Zoka.TestSuite.Abstraction
{
	/// <summary>Delegate which is used for collecting services during building up service provider (DependencyInjection)</summary>
	public delegate IServiceCollection						CollectServicesDelegate(IServiceCollection _service_collection);

	/// <summary>Delegate which is used for configuring services once the service collection has been finished and services are about to be configured before fisrt use if necessary (DependenciInjection)</summary>
	public delegate IServiceProvider						ConfigureServicesDelegate(IServiceProvider _service_provider);
}
