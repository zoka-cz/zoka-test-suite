using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Zoka.TestSuite.Abstraction;
using Zoka.TestSuite.Abstraction.XMLHelpers;
using Zoka.ZScript;

namespace Zoka.TestSuite.DataStorageActions
{
	/// <summary>This action will perform regex on passed string and matches are stored into DataStorage</summary>
	public class StoreRegexMatchesTestAction : IPlaylistAction
	{
		/// <summary>Action type name</summary>
		public const string									ACTION_TYPE_NAME = "StoreRegexMatches";

		/// <summary>Name of the action</summary>
		public string?										Name { get; private set; }
		/// <summary>Id of the action</summary>
		public string?										Id { get; private set; }

		/// <summary>Description of the action</summary>
		public string?										Description { get; private set; }

		private Dictionary<int, string>						m_Targets;
		private string										m_Source;
		private string										m_Regex;

		/// <summary>Constructor</summary>
		public StoreRegexMatchesTestAction(Dictionary<int, string> _target, string _source, string _regex)
		{
			m_Targets = _target;
			m_Source = _source;
			m_Regex = _regex;
		}

		/// <inheritdoc />
		public EPlaylistActionResultInstruction				PerformAction(DataStorages _data_storages, IServiceProvider _service_provider)
		{
			
			return EPlaylistActionResultInstruction.Fail;
			//var source_val = ZScriptExpressionParser.ParseScriptExpression(m_Source).EvaluateExpressionToValue(_data_storages, _service_provider) as string;

			//var token = JToken.Parse(source_val ?? throw new InvalidOperationException($"Error getting source value ({m_Source}"));
			//var val = token.SelectToken(m_JsonPath)?.ToString() ?? throw new Exception($"Could not find any value in json on specified path ({m_JsonPath}");

			//_data_storages.Store(m_Target, val);

			//_service_provider.GetService<ILogger<DataStorageFromJsonTestAction>>()?.LogDebug($"Stored value ({val}) into {m_Target}");

			//return 0;
		}

		/// <summary>Returns the name of the test action</summary>
		public override string								ToString()
		{
			return $"StoreRegexMatchesTestAction test action {(Description != null ? $"({Description})" : "")}";
		}

		#region XML Loading

		/// <summary>Parse the action from the XML Element</summary>
		public static StoreRegexMatchesTestAction?		ParseFromXmlElement(FileInfo _src_file, XElement _x_element, List<IFunctionAction> _imported_functions, IServiceProvider _service_provider)
		{
			if (_x_element.Name != ACTION_TYPE_NAME)
			{
				throw new ZTSXmlException($"Expected {ACTION_TYPE_NAME} element name, but got {_x_element.Name}");
			}

			var tgt_attrs = _x_element.Attributes().Where(a => a.Name.LocalName.StartsWith("target"));

			var action = new StoreRegexMatchesTestAction(
				_target: tgt_attrs.ToDictionary(ta => int.Parse(ta.Name.LocalName.Substring(6)), ta => ta.Value),
				_source: _x_element.ReadAttr<string>("source", _src_file, true),
				_regex: _x_element.ReadAttr<string>("regex", _src_file, true)
				)
			{
				Description = _x_element.ReadDescAttr(_src_file, false),
				Id = _x_element.ReadIdAttr(_src_file, false),
				Name = _x_element.ReadNameAttr(_src_file, false)
			};

			return action;
		}


		#endregion // XML Loading
	}
}
