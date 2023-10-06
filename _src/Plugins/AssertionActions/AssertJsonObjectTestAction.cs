using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using FluentAssertions.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Zoka.TestSuite.Abstraction;
using Zoka.TestSuite.Abstraction.ScriptHelpes;
using Zoka.TestSuite.Abstraction.XMLHelpers;

namespace Zoka.TestSuite.AssertionActions
{
	/// <summary>Test action, which takes json object and asserted json object (as tet between elements or from external file, ZScript replacements allowed) and finds out, whether the json object contains the asserted json object as its subtree</summary>
	public class AssertJsonObjectTestAction : IPlaylistAction
	{
		/// <summary>Action type name</summary>
		public const string									ACTION_TYPE_NAME = "AssertJsonObject";

		/// <summary>Id of the action</summary>
		public string?										Id { get; private set; }
		/// <summary>Description of the action</summary>
		public string?										Description { get; private set; }
		/// <summary>Name of the action</summary>
		public string?										Name { get; private set; }

		private string										m_JsonObjectExpr;
		private string?										m_AssertedJsonFile;
		private string?										m_AssertedJson;

		/// <summary>Constructor</summary>
		public AssertJsonObjectTestAction(string _json_object_expr, string? _asserted_json_file, string? _asserted_json)
		{
			m_JsonObjectExpr = _json_object_expr;
			m_AssertedJsonFile = _asserted_json_file;
			m_AssertedJson = _asserted_json;
		}

		/// <inheritdoc />
		public EPlaylistActionResultInstruction				PerformAction(DataStorages _data_storages, IServiceProvider _service_provider)
		{
			var script_helper = new ScriptHelper(_data_storages, _service_provider);

			var json_obj = _data_storages.GetObjectFromDataStorage(m_JsonObjectExpr) as string;

			var asserted_json = GetJson(script_helper);

			var asserted_reader = new JsonTextReader(new StringReader(asserted_json));
			var asserted_obj = JToken.Load(asserted_reader);

			var obj_reader = new JsonTextReader(new StringReader(json_obj ?? throw new InvalidOperationException("Json object to check could not be read.")));
			var obj = JToken.Load(obj_reader);

			_service_provider.GetService<ILogger<AssertJsonObjectTestAction>>()?.LogInformation($"Testing whether the JSON:{Environment.NewLine}{json_obj}{Environment.NewLine}contains the subtree of JSON:{Environment.NewLine}{asserted_json}{Environment.NewLine}");

			obj.Should().ContainSubtree(asserted_obj);

			return EPlaylistActionResultInstruction.NoInstruction;
		}

		private string										GetJson(ScriptHelper _script_helper)
		{
			string? json = null;
			if (!string.IsNullOrWhiteSpace(m_AssertedJsonFile))
			{
				var json_filename = _script_helper.EvaluateStringReplacements(m_AssertedJsonFile);
				var fi = new FileInfo(json_filename);
				if (fi.Exists)
				{
					json = File.ReadAllText(fi.FullName);
				}
				else
					throw new Exception($"The asserted_json_from could ({m_AssertedJsonFile}) not be evaluated into filename which exist (evaluated into: {fi.FullName})");
			}
			if (json == null && !string.IsNullOrWhiteSpace(m_AssertedJson))
				json = m_AssertedJson;

			if (json != null)
			{
				json = _script_helper.EvaluateStringReplacements(json);
			}
			if (json == null)
				throw new Exception("No asserted json is present.");
			return json;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"AssertJsonObject test action {(Description != null ? $"({Description})" : "")}";
		}

		#region XML Loading

		/// <summary>Loads the assert regex match action from XML Element</summary>
		public static AssertJsonObjectTestAction?			ParseFromXmlElement(FileInfo _src_file, XElement _x_element, List<IFunctionAction> _imported_functions, IServiceProvider _service_provider)
		{
			if (_x_element.Name != ACTION_TYPE_NAME)
			{
				throw new ZTSXmlException($"Expected {ACTION_TYPE_NAME} element name, but got {_x_element.Name}");
			}

			var asserted_json_filename = _x_element.ReadAttr<string?>("asserted_json_from", _src_file, false);
			if (asserted_json_filename != null)
				asserted_json_filename = Path.Combine(_src_file.DirectoryName!, asserted_json_filename);
			var assert_action = new AssertJsonObjectTestAction(
				_x_element.ReadAttr<string>("json_object", _src_file, true), 
				asserted_json_filename,
				!_x_element.IsEmpty && !string.IsNullOrWhiteSpace(_x_element.Value) ? _x_element.Value : null)
			{
				Name = _x_element.ReadNameAttr(_src_file, false),
				Description = _x_element.ReadDescAttr(_src_file, false),
				Id = _x_element.ReadIdAttr(_src_file, false)
			};

			return assert_action;
		}

		#endregion // XML Loading
	}
}
