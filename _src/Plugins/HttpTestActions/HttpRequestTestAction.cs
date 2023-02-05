using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zoka.TestSuite.Abstraction;
using Zoka.TestSuite.Abstraction.XMLHelpers;

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
		/// <summary>If set, it uses this basic auth param</summary>
		public string?										BasicAuth { get; private set; }
		/// <summary>The content</summary>
		public string?										Content { get; private set; }
		/// <summary>Content into</summary>
		public string?										ContentInto { get; private set;}
		/// <summary>Http method</summary>
		public HttpMethod									Method { get; private set; } = HttpMethod.Get;

		/// <summary>Constructor</summary>
		protected HttpRequestTestAction(string _name, string _url)
		{
			Name = _name;
			Url = _url;
		}

		/// <summary>Will perform action</summary>
		public int											PerformAction(DataStorages _data_storages, IServiceProvider _service_provider)
		{
			//var logger = _service_provider.GetService<ILogger<HttpRequestTestAction>>();

			//HttpClient client = new HttpClient();
			//if (ServerBaseUrl != null)
			//{
			//	var server_base_url = ScriptExpressionParser.ParseScriptExpression(ServerBaseUrl).EvaluateExpressionToValue(_data_storages, _service_provider) as string;
			//	client.BaseAddress = new Uri(server_base_url ?? throw new InvalidOperationException($"Could not evaluate server parameter ({ServerBaseUrl})"));
			//}

			//var url = ScriptExpressionParser.EvaluateScriptReplacements(Url, _data_storages, _service_provider);
			//HttpRequestMessage request = new HttpRequestMessage(Method, url);
			//logger.LogInformation($"Sending request: {Method.ToString().ToUpper()} {url}");

			//if (BasicAuth != null)
			//{
			//	var basic_auth = ScriptExpressionParser.ParseScriptExpression(BasicAuth).EvaluateExpressionToValue(_data_storages, _service_provider) as string;
			//	client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basic_auth?.Base64Encode() ?? throw new InvalidOperationException($"Could not evaluate basic_auth parameter ({BasicAuth})"));
			//	logger.LogInformation($"Authorizing as Basic {basic_auth}");
			//}

			//if (Content != null)
			//{
			//	var content = ScriptExpressionParser.EvaluateScriptReplacements(Content, _data_storages, _service_provider);
			//	request.Content = new StringContent(content, Encoding.UTF8, "application/json");
			//	logger.LogInformation($"Message content: {content}");
			//}

			//var resp = client.Send(request);
			//logger.LogInformation($"Response received: {resp.StatusCode:D} {resp.StatusCode:G}");
			//var resp_content = resp.Content.ReadAsStringAsync().Result;
			//if (!string.IsNullOrWhiteSpace(resp_content))
			//	logger.LogInformation($"ResponseContent:{Environment.NewLine}{resp_content}");

			//if (!resp.IsSuccessStatusCode)
			//{
			//	throw new Exception($"Request to {request.RequestUri} has failed with HttpStatusCode: {resp.StatusCode:D} {resp.StatusCode:G}");
			//}

			//if (ContentInto != null)
			//	_data_storages.Store(ContentInto, resp_content);

			//return 0;
			return 0;
		}

		/// <summary>To string</summary>
		public override string								ToString()
		{
			return $"HttpRequest test action {(Description != null ? $"({Description})" : "")}";
		}


		#region XML Loading

		/// <summary>Parse the action from the XML Element</summary>
		public static HttpRequestTestAction?				ParseFromXmlElement(FileInfo _src_file, XElement _x_element, IServiceProvider _service_provider)
		{
			var name = _x_element.ReadNameAttr(_src_file, false);
			var desc = _x_element.ReadDescAttr(_src_file, false);
			var id = _x_element.ReadIdAttr(_src_file, false);
			var url = _x_element.ReadAttr<string>("url", _src_file, true);

			var http_req = new HttpRequestTestAction(name, url)
			{
				ServerBaseUrl = _x_element.ReadAttr<string?>("server", _src_file, false),
				ContentInto = _x_element.ReadAttr<string?>("content_into", _src_file, false),
				BasicAuth = _x_element.ReadAttr<string?>("basic_auth", _src_file, false),
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
