using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Jint;
using Zoka.TestSuite.Abstraction;
using Zoka.TestSuite.Abstraction.ScriptHelpes;
using Zoka.TestSuite.Abstraction.XMLHelpers;

namespace Zoka.TestSuite.BasicTestActions
{
	internal class BreakAction : IPlaylistAction
	{
		/// <summary>XML element name</summary>
		public const string									ACTION_TYPE_NAME = "Break";

		/// <summary>Name of the action</summary>
		public string										Name { get; }
		/// <summary>Id of the action</summary>
		public string										Id { get; } 
		/// <summary>Description of the action</summary>
		public string										Description { get; }

		/// <summary>Constructor</summary>
		private BreakAction(string _name, string _id, string _description)
		{
			Name = _name;
			Id = _id;
			Description = _description;
		}

		/// <inheritdoc />
		public EPlaylistActionResultInstruction PerformAction(DataStorages _data_storages, IServiceProvider _service_provider)
		{
			var logger = _service_provider.GetRequiredService<ILogger<IfAction>>();
			logger.LogInformation("Breaking from closest loop");
			return EPlaylistActionResultInstruction.BreakLoop;
		}

		/// <summary>To string</summary>
		public override string								ToString()
		{
			return $"Function {Name} {(Description != null ? $"({Description})" : "")}";
		}

		#region XML Loading

		/// <summary>Parse Action from XmlElement</summary>
		public static BreakAction								ParseFromXmlElement(FileInfo _src_file, XElement _x_element, List<IFunctionAction> _imported_functions, IServiceProvider _service_provider)
		{
			var name = _x_element.ReadNameAttr(_src_file, false);
			var desc = _x_element.ReadDescAttr(_src_file, false);
			var id = _x_element.ReadIdAttr(_src_file, false);
			var func = new BreakAction(name, id, desc);
			return func;
		}


		#endregion // XML Loading

	}
}
