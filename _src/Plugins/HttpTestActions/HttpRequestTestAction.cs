using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Jint;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Stubble.Core.Builders;
using Zoka.TestSuite.Abstraction;
using Zoka.TestSuite.Abstraction.ScriptHelpes;
using Zoka.TestSuite.Abstraction.XMLHelpers;
using Zoka.TestSuite.HttpTestActions.AuthConfiguration;

namespace Zoka.TestSuite.HttpTestActions
{
	/// <summary>Playlist action which sends Http request to the server and receives the response</summary>
	public class HttpRequestTestAction : IPlaylistAction
	{
		/// <summary>Action type name</summary>
		public const string									ACTION_TYPE_NAME = "HttpRequest";

		/// <summary>Name of the action</summary>
		public string?										Name { get; }

		/// <summary>Id of the action</summary>
		public string?										Id { get; private set; }
		/// <summary>Description of the action</summary>
		public string?										Description { get; private set; }
		/// <summary>Server base url</summary>
		public string?										ServerBaseUrl { get; private set; }
		/// <summary>Relative Url to call on server</summary>
		public string										Url { get; private set; }
		/// <summary>If set, it uses auth param from storage to configure authentication</summary>
		public string?										Auth { get; private set; }
		/// <summary>The content</summary>
		public string?										Content { get; private set; }
		/// <summary>Result into</summary>
		public string?										ResultInto { get; private set;}
		/// <summary>Store result as (string or JObject)</summary>
		public string?										ResultAs { get; private set; }
		/// <summary>Http method</summary>
		public HttpMethod									Method { get; private set; } = HttpMethod.Get;

		/// <summary>Constructor</summary>
		protected HttpRequestTestAction(string _name, string _url)
		{
			Name = _name;
			Url = _url;
		}

		/// <summary>Will perform action</summary>
		public EPlaylistActionResultInstruction				PerformAction(DataStorages _data_storages, IServiceProvider _service_provider)
		{
			var logger = _service_provider.GetService<ILogger<HttpRequestTestAction>>();
			var script_helper = new ScriptHelper(_data_storages, _service_provider);

			HttpClient client = new HttpClient();
			if (ServerBaseUrl != null)
			{
				var eng = new Engine();
				var server_base_url = script_helper.EvaluateStringReplacements(ServerBaseUrl);
				client.BaseAddress = new Uri(server_base_url ?? throw new InvalidOperationException($"Could not evaluate server parameter ({ServerBaseUrl})"));
			}

			var url = script_helper.EvaluateStringReplacements(Url);
			HttpRequestMessage request = new HttpRequestMessage(Method, url);
			logger?.LogInformation($"Sending request: {Method.ToString().ToUpper()} {url}");

			if (Auth != null)
			{
				ResolveAuth(client, _data_storages, _service_provider);
			}
			//if (BasicAuth != null)
			//{
			//	var basic_auth = ZScriptExpressionParser.ParseScriptExpression(BasicAuth).EvaluateExpressionToValue(_data_storages, _service_provider) as string;
			//	var auth_bytes = Encoding.UTF8.GetBytes(basic_auth);
			//	client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(auth_bytes));
			//	logger?.LogInformation($"Authorizing as Basic {basic_auth}");
			//}

			if (Content != null)
			{
				var content = script_helper.EvaluateStringReplacements(Content);
				request.Content = new StringContent(content, Encoding.UTF8, "application/json");
				logger?.LogInformation($"Message content: {content}");
			}

			var resp = client.SendAsync(request).Result;
			logger?.LogInformation($"Response received: {resp.StatusCode:D} {resp.StatusCode:G}");
			var resp_content = resp.Content.ReadAsStringAsync().Result;
			if (!string.IsNullOrWhiteSpace(resp_content))
				logger?.LogInformation($"ResponseContent:{Environment.NewLine}{resp_content}");

			if (!resp.IsSuccessStatusCode)
			{
				throw new Exception($"Request to {request.RequestUri} has failed with HttpStatusCode: {resp.StatusCode:D} {resp.StatusCode:G}");
			}

			if (ResultInto != null)
			{
				if (ResultAs == null || ResultAs == "string")
					_data_storages.Store(ResultInto, resp_content);
				else if (ResultAs == "JObject")
				{
					var jobj = JsonConvert.DeserializeObject(resp_content);
					if (jobj == null)
						throw new Exception($"Error storing result as JObject, could not been converted");
					_data_storages.Store(ResultInto, jobj);
				}
				else
					throw new Exception("ResultAs can be only \"string\" or \"JObject\"");
			}

			return EPlaylistActionResultInstruction.NoInstruction;
		}

		private void										ResolveAuth(HttpClient _http_client, DataStorages _data_storages, IServiceProvider _service_provider)
		{
			if (Auth == null)
				return;
			var auth = _data_storages.GetObjectFromDataStorage(Auth);

			// on the first usage it is configuration object (IConfigurationSection), thus we need to parse it first before other using and store it again as actual configuration
			if (auth is IConfigurationSection auth_section)
			{
				if (auth_section.GetSection("Type").Value == "JWT")
				{
					var jwt_auth_config = auth_section.Get<JWTAuthConfiguration>();
					_data_storages.Store(Auth, jwt_auth_config);
				}
			}

			var auth_config = _data_storages.GetObjectFromDataStorage(Auth) as IAuthConfiguration;
			if (auth_config == null)
				throw new Exception($"{Auth} was not resolved to the IAuthConfiguration type");
			
			auth_config.ResolveAuth(_http_client, _data_storages, _service_provider);
		}



		/// <summary>To string</summary>
		public override string								ToString()
		{
			return $"HttpRequest test action {(Description != null ? $"({Description})" : "")}";
		}


		#region XML Loading

		/// <summary>Parse the action from the XML Element</summary>
		public static HttpRequestTestAction?				ParseFromXmlElement(FileInfo _src_file, XElement _x_element, List<IFunctionAction> _imported_functions, IServiceProvider _service_provider)
		{
			var name = _x_element.ReadNameAttr(_src_file, false);
			var desc = _x_element.ReadDescAttr(_src_file, false);
			var id = _x_element.ReadIdAttr(_src_file, false);
			var url = _x_element.ReadAttr<string>("url", _src_file, true);

			var http_req = new HttpRequestTestAction(name, url)
			{
				ServerBaseUrl = _x_element.ReadAttr<string?>("server", _src_file, false),
				ResultInto = _x_element.ReadAttr<string?>("result_into", _src_file, false),
				ResultAs = _x_element.ReadAttr<string?>("result_as", _src_file, false),
				Auth = _x_element.ReadAttr<string?>("auth", _src_file, false),
				Method = new HttpMethod(_x_element.ReadAttr<string?>("method", _src_file, false) ?? "GET")
			};

			if (!string.IsNullOrWhiteSpace(_x_element.Value))
				http_req.Content = _x_element.Value;

			var content_from = _x_element.ReadAttr<string?>("content_from", _src_file, false);
			if (content_from != null)
			{
				http_req.Content = File.ReadAllText(Path.Combine(_src_file.DirectoryName!, content_from));
			}

			return http_req;
		}


		#endregion // XML Loading
	}
}
