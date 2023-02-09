using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Zoka.TestSuite.Abstraction;
using Zoka.TestSuite.Abstraction.XMLHelpers;
using Zoka.ZScript;

namespace Zoka.TestSuite.AssertionActions
{
	/// <summary>Assert regex match test action</summary>
	public class AssertRegexMatchTestAction : IPlaylistAction
	{
		/// <summary>Action type name</summary>
		public const string									ACTION_TYPE_NAME = "AssertRegexMatch";

		/// <summary>Id of the action</summary>
		public string?										Id { get; private set; }
		/// <summary>Description of the action</summary>
		public string?										Description { get; private set; }
		/// <summary>Name of the action</summary>
		public string?										Name { get; private set; }

		private string										m_ToCheckExpr;
		private string										m_AssertedRegex;

		/// <summary>Constructor</summary>
		public AssertRegexMatchTestAction(string _asserted_regex, string _to_check_expr)
		{
			m_AssertedRegex = _asserted_regex;
			m_ToCheckExpr = _to_check_expr;
		}

		/// <inheritdoc />
		public int											PerformAction(DataStorages _data_storages, IServiceProvider _service_provider)
		{

			int pos = 0;
			var to_check_expr = ZScriptExpressionParser.ParseScriptExpression(m_ToCheckExpr, ref pos);

			var to_check_val = to_check_expr.EvaluateExpressionToValue(_data_storages, _service_provider) as string;

			var regex = new Regex(m_AssertedRegex);

			if (!regex.IsMatch(to_check_val ?? throw new InvalidOperationException($"Could not evaluate to_check attribute (value={to_check_expr.OriginalExpression})")))
				throw new Exception($"The value {to_check_val} does not match the regex pattern {m_AssertedRegex}");

			return 0;
		}


		/// <summary>To string</summary>
		public override string								ToString()
		{
			return $"AssertRegexMatchTestAction test action {(Description != null ? $"({Description})" : "")}";
		}

		#region XML Loading

		/// <summary>Loads the assert regex match action from XML Element</summary>
		public static AssertRegexMatchTestAction?			ParseFromXmlElement(FileInfo _src_file, XElement _x_element, IServiceProvider _service_provider)
		{
			if (_x_element.Name != ACTION_TYPE_NAME)
			{
				throw new ZTSXmlException($"Expected {ACTION_TYPE_NAME} element name, but got {_x_element.Name}");
			}

			var assert_action = new AssertRegexMatchTestAction(_x_element.ReadAttr<string>("asserted_regex", _src_file, true), _x_element.ReadAttr<string>("to_check", _src_file, true))
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
