using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Zoka.ZScript;
using Zoka.ZScript.BaseTypesFunctions;

namespace Zoka.TestSuite.Abstraction
{
	/// <summary>Configuring the ZTS</summary>
	public static class ZokaTestSuiteAbstractionsExtensions
	{
		/// <summary>Will collect all services used by ZTS</summary>
		public static IServiceCollection					CollectZTSServices(this IServiceCollection _services)
		{
			_services.AddSingleton<ZScriptFunctionFactory>();

			return _services;
		}

		/// <summary>Will configure all services used by ZTS</summary>
		public static IServiceProvider						ConfigureZTSServices(this IServiceProvider _service_provider)
		{
			_service_provider.ConfigureBaseTypesFunctions();

			return _service_provider;
		}
	}
}
