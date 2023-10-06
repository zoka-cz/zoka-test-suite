using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Zoka.TestSuite.Abstraction;
using Zoka.TestSuite.Abstraction.ScriptHelpes;

namespace Zoka.TestSuite.HttpTestActions.AuthConfiguration
{
	internal class JWTAuthConfiguration : IAuthConfiguration
	{
		public string										Type { get; set; } = null!;
		public string										LoginURL { get; set; } = null!;
		public string										LoginMethod { get; set; } = null!;
		public string										LoginBody { get; set; } = null!;
		public LoginResponsePaths?							LoginResponse { get; set; } = null!;
		public string?										RefreshURL { get; set; } = null;
		public string?										RefreshMethod { get; set; }

		#region Actual tokens

		public string?										Token { get; set; } = null;
		public DateTime?									TokenValidation { get; set; } = null;
		public string?										RefreshToken { get; set; } = null;
		public DateTime?									RefreshTokenValidation { get; set; } = null;

		#endregion // Actual tokens

		#region Resolving auth

		public void											ResolveAuth(HttpClient _http_client, DataStorages _data_storages, IServiceProvider _service_provider)
		{
			if (Token == null || TokenValidation < DateTime.UtcNow)
			{
				Login(_data_storages, _service_provider);
			}

			if (Token != null && TokenValidation > DateTime.UtcNow)
			{
				_http_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
			}

		}

		private void										Login(DataStorages _data_storages, IServiceProvider _service_provider)
		{
			var script_helper = new ScriptHelper(_data_storages, _service_provider);

			var login_url = new Uri(script_helper.EvaluateStringReplacements(LoginURL));
			var http_client = new HttpClient();
			http_client.BaseAddress = login_url;
			var msg = new HttpRequestMessage
			{
				Method = new HttpMethod(LoginMethod),
				Content = new StringContent(script_helper.EvaluateStringReplacements(LoginBody), Encoding.UTF8, "application/json"),
				RequestUri = login_url
			};

			var response = http_client.SendAsync(msg).Result;
			if (!response.IsSuccessStatusCode)
				throw new Exception("Error logging in");
			var resp_cnt = response.Content.ReadAsStringAsync().Result;
			JObject jobj = JObject.Parse(resp_cnt);
			if (LoginResponse != null)
			{
				if (!string.IsNullOrWhiteSpace(LoginResponse.TokenPath))
					Token = jobj.SelectToken(LoginResponse.TokenPath)?.Value<string>();
				if (!string.IsNullOrWhiteSpace(LoginResponse.TokenExpirationPath))
					TokenValidation = jobj.SelectToken(LoginResponse.TokenExpirationPath)?.Value<DateTime>();
				if (!string.IsNullOrWhiteSpace(LoginResponse.RefreshTokenPath))
					RefreshToken = jobj.SelectToken(LoginResponse.RefreshTokenPath)?.Value<string>();
				if (!string.IsNullOrWhiteSpace(LoginResponse.RefreshTokenExpirationPath))
					RefreshTokenValidation = jobj.SelectToken(LoginResponse.RefreshTokenExpirationPath)?.Value<DateTime>();
			}
		}

		#endregion // Resolving auth

	}
}
