using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Zoka.TestSuite.Abstraction;

namespace Zoka.TestSuite.HttpTestActions.AuthConfiguration
{
	internal interface IAuthConfiguration
	{
		string Type { get; set; }

		void ResolveAuth(HttpClient _http_client, DataStorages _data_storages, IServiceProvider _service_provider);
	}
}
