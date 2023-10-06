using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zoka.TestSuite.Abstraction;
using Zoka.TestSuite.Abstraction.ScriptHelpes;
using Zoka.TestSuite.Abstraction.XMLHelpers;

namespace Zoka.TestSuite.DataStorageActions
{
	/// <summary>Test action, which parses source ZScript expression and stores it into target variable in data storage</summary>
	public class DataStorageTestAction : IPlaylistAction
	{
		/// <summary>Action type name</summary>
		public const string									ACTION_TYPE_NAME = "DataStorage";

		/// <summary>Name of the action</summary>
		public string?										Name { get; private set; }
		/// <summary>Id of the action</summary>
		public string?										Id { get; private set; }

		/// <summary>Description of the action</summary>
		public string?										Description { get; private set; }
		
		private string										m_Target;
		private string										m_Source;

		/// <summary>Constructor</summary>
		protected DataStorageTestAction(string _source_expr, string _target)
		{
			m_Source = _source_expr;
			m_Target = _target;
		}

		/// <inheritdoc />
		public EPlaylistActionResultInstruction				PerformAction(DataStorages _data_storages, IServiceProvider _service_provider)
		{
			var script_helper = new ScriptHelper(_data_storages, _service_provider);

			var source_val = script_helper.EvaluateStringReplacements(m_Source);
			_data_storages.Store(m_Target, source_val);

			_service_provider.GetService<ILogger<DataStorageTestAction>>()?.LogDebug($"Stored value ({source_val}) into {m_Target}");

			return EPlaylistActionResultInstruction.NoInstruction;
		}

		/// <inheritdoc />
		public override string								ToString()
		{
			return $"DataStorageFromScriptPlaylistAction test action {(Description != null ? $"({Description})" : "")}";
		}

		#region XML Loading


		/// <summary>Parse the action from the XML Element</summary>
		public static DataStorageTestAction?				ParseFromXmlElement(FileInfo _src_file, XElement _x_element, List<IFunctionAction> _imported_functions, IServiceProvider _service_provider)
		{
			if (_x_element.Name != ACTION_TYPE_NAME)
			{
				throw new ZTSXmlException($"Expected {ACTION_TYPE_NAME} element name, but got {_x_element.Name}");
			}

			var action = new DataStorageTestAction(
				_source_expr: _x_element.ReadAttr<string>("source", _src_file, true),
				_target: _x_element.ReadAttr<string>("target", _src_file, true)
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
