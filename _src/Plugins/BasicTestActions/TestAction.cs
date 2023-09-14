using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zoka.TestSuite.Abstraction;
using Zoka.TestSuite.Abstraction.XMLHelpers;
using Zoka.ZScript;

namespace Zoka.TestSuite.BasicTestActions
{
	/// <summary>Test action</summary>
	public class TestAction : IPlaylistAction
	{
		/// <summary>XML element name</summary>
		public const string									ACTION_TYPE_NAME = "Test";
		
		private List<IPlaylistAction>						ArrangeActions { get; } = new List<IPlaylistAction>();
		private List<IPlaylistAction>						ActActions { get; } = new List<IPlaylistAction>();
		private List<IPlaylistAction>						AssertActions { get; } = new List<IPlaylistAction>();
		private List<string>								Exports { get; } = new List<string>();

		/// <summary>Name of the action</summary>
		public string										Name { get; }
		/// <summary>Id of the action</summary>
		public string										Id { get; }
		/// <summary>Description of the action</summary>
		public string										Description { get; }

		/// <summary>Constructor</summary>
		public TestAction(string _id, string _name, string _description)
		{
			Id = _id;
			Name = _name;
			Description = _description;
		}

		private EPlaylistActionResultInstruction			PerformTestPhase(List<IPlaylistAction> _actions, string _phase_name, DataStorages _data_storages, IServiceProvider _service_provider)
		{
			if (_actions.Count > 0)
			{
				var logger = _service_provider.GetService<ILogger<TestAction>>();
				logger?.Log(LogLevel.Information, $"Starting {_phase_name} phase of {this}");
				foreach (var action in _actions)
				{
					logger?.Log(LogLevel.Information, $"Running action {action}");
					var ret = action.PerformAction(_data_storages, _service_provider);
					if (ret == EPlaylistActionResultInstruction.Fail)
						return ret;
				}
			}

			return EPlaylistActionResultInstruction.NoInstruction;
		}

		/// <inheritdoc />
		public EPlaylistActionResultInstruction				PerformAction(DataStorages _data_storages, IServiceProvider _service_provider)
		{
			_data_storages.Push(new DataStorage());
			var ret = PerformTestPhase(ArrangeActions, "Arrange", _data_storages, _service_provider);
			if (ret == EPlaylistActionResultInstruction.Fail)
				return ret;
			ret = PerformTestPhase(ActActions, "Act", _data_storages, _service_provider);
			if (ret == EPlaylistActionResultInstruction.Fail)
				return ret;
			ret = PerformTestPhase(AssertActions, "Assert", _data_storages, _service_provider);
			if (ret == EPlaylistActionResultInstruction.Fail)
				return ret;

			var storage_to_remove = _data_storages.Pop();

			foreach (var export_var in Exports)
			{
				if (storage_to_remove.ContainsKey(export_var))
					_data_storages.Store(export_var, storage_to_remove[export_var]);
				else
				{
					_service_provider.GetService<ILogger<TestAction>>()?.LogWarning($"Instructed to export variable {export_var}, but not found in current context.");
				}
			}

			return EPlaylistActionResultInstruction.NoInstruction;
		}

		/// <summary>To string</summary>
		public override string								ToString()
		{
			return $"Test {Name} {(Description != null ? $"({Description})" : "")}";
		}

		#region XML Loading

		/// <summary>Parse Action from XmlElement</summary>
		public static TestAction							ParseFromXmlElement(FileInfo _src_file, XElement _x_element, List<IFunctionAction> _imported_functions, IServiceProvider _service_provider)
		{
			var name = _x_element.ReadNameAttr(_src_file, true);
			var desc = _x_element.ReadDescAttr(_src_file, false);
			var id = _x_element.ReadIdAttr(_src_file, false);
			

			var test = new TestAction(id, name, desc);


			ParseActions(_x_element.Element("Arrange"), test.ArrangeActions, _imported_functions, _service_provider, _src_file);
			ParseActions(_x_element.Element("Act"), test.ActActions, _imported_functions, _service_provider, _src_file);
			ParseActions(_x_element.Element("Assert"), test.AssertActions, _imported_functions, _service_provider, _src_file);

			var export_el = _x_element.Element("Export");
			if (export_el != null)
			{
				foreach (var export_item in export_el.Elements("Item"))
				{
					test.Exports.Add(export_item.Value.Trim());
				}
			}

			return test;
		}

		private static void									ParseActions(XElement? _x_element, List<IPlaylistAction> _actions, List<IFunctionAction> _imported_functions, IServiceProvider _service_provider, FileInfo _src_file)
		{
			if (_x_element == null)
				return;

			var test_action_factory = _service_provider.GetRequiredService<TestPlaylistActionFactory>();
			foreach (var x_action_element in _x_element.Elements())
			{
				var action = test_action_factory.LoadFromXmlElement(_src_file, x_action_element, _imported_functions, _service_provider);
				if (action != null)
					_actions.Add(action);
				else
				{
					throw new Exception($"Unknown action {x_action_element.Name}");
				}
			}
		}


		#endregion // XML Loading
	}
}
