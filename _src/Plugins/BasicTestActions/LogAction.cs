using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Zoka.TestSuite.Abstraction;
using Zoka.TestSuite.Abstraction.XMLHelpers;

namespace Zoka.TestSuite.BasicTestActions
{
	/// <summary>Will log passed information onto the console</summary>
	public class LogAction : IPlaylistAction
	{
		/// <summary>XML element name</summary>
		public const string									ACTION_TYPE_NAME = "Log";

		/// <summary>Name of the action</summary>
		public string?										Name { get; private set; }
		/// <summary>Id of the action</summary>
		public string?										Id { get; private set; }
		/// <summary>Description of the action</summary>
		public string?										Description { get; private set; }

		/// <summary>Expression in ZScript, which resolved is printed on the console</summary>
		private string										LogExpression { get; set; } = null!;

		/// <inheritdoc />
		public EPlaylistActionResultInstruction				PerformAction(DataStorages _data_storages, IServiceProvider _service_provider)
		{
			throw new NotImplementedException();

			//var result = ZScriptExpressionParser.EvaluateScriptReplacements(LogExpression, _data_storages, _service_provider);

			//Console.WriteLine(result);
			//var logger = _service_provider.GetService<ILogger<LogAction>>();
			//logger?.LogInformation(result);

			//return EPlaylistActionResultInstruction.NoInstruction;
		}

		/// <summary>To string</summary>
		public override string								ToString()
		{
			return $"Function {Name} {(Description != null ? $"({Description})" : "")}";
		}

		/// <summary>Parse Action from XmlElement</summary>
		public static LogAction								ParseFromXmlElement(FileInfo _src_file, XElement _x_element, List<IFunctionAction> _imported_functions, IServiceProvider _service_provider)
		{
			string log_expression;
			var el_log_expression = _x_element.Attribute("log");
			if (el_log_expression == null)
				log_expression = _x_element.Value;
			else
				log_expression = el_log_expression.Value;

			var log = new LogAction()
			{
				Id = _x_element.ReadIdAttr(_src_file, false),
				Name = _x_element.ReadNameAttr(_src_file, false),
				Description = _x_element.ReadDescAttr(_src_file, false),
				LogExpression = log_expression
			};

			return log;
		}


	}
}
