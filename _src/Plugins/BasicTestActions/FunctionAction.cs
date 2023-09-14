using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zoka.TestSuite.Abstraction;
using Zoka.TestSuite.Abstraction.XMLHelpers;
using Zoka.ZScript;

namespace Zoka.TestSuite.BasicTestActions
{
	/// <summary>Test action</summary>
	public class FunctionAction : IFunctionAction
	{
		class FunctionParameter
		{
			public string									Name { get; set; } = null!;
			public string									PassedParameterExpression { get; set; } = null!;
			public Type										ParameterType { get; set; } = null!;
			public bool										IsOutput { get; set; }

			public override string ToString()
			{
				return $"FunctionParameter (\"{Name}\", Type: {ParameterType.Name}, Expr: {PassedParameterExpression}";
			}
		}

		/// <summary>XML element name</summary>
		public const string									ACTION_TYPE_NAME = "Function";
		
		private readonly List<IPlaylistAction>				m_Actions = new List<IPlaylistAction>();
		private readonly List<string>						m_Exports = new List<string>();
		private readonly List<FunctionParameter>			m_Parameters = new List<FunctionParameter>();

		/// <summary>Name of the action</summary>
		public string										Name { get; }
		/// <summary>Id of the action</summary>
		public string										Id { get; }
		/// <summary>Description of the action</summary>
		public string										Description { get; }


		/// <summary>Constructor</summary>
		private FunctionAction(string _id, string _name, string _description, IEnumerable<IPlaylistAction> _actions, IEnumerable<string> _exports, IEnumerable<FunctionParameter> _parameters)
		{
			Id = _id;
			Name = _name;
			Description = _description;
			m_Actions.AddRange(_actions);
			m_Exports.AddRange(_exports);
			m_Parameters.AddRange(_parameters);
		}

		/// <inheritdoc />
		public EPlaylistActionResultInstruction				PerformAction(DataStorages _data_storages, IServiceProvider _service_provider)
		{
			ResolveParameters(_data_storages, _service_provider);

			_data_storages.Push(new DataStorage());
			var logger = _service_provider.GetService<ILogger<TestAction>>();
			foreach (var action in m_Actions)
			{
				logger?.Log(LogLevel.Information, $"Running action {action}");
				var ret = action.PerformAction(_data_storages, _service_provider);
				if (ret == EPlaylistActionResultInstruction.Fail)
					return ret;
			}

			var storage_to_remove = _data_storages.Pop();

			foreach (var export_var in m_Exports)
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

		/// <inheritdoc />
		public void											AddPassedParameter(string _param_name, string _param_value_expression)
		{
			var param = m_Parameters.FirstOrDefault(p => p.Name == _param_name);
			if (param == null)
				throw new Exception($"Passing parameter {_param_name} into function {Name} which is not defined in input parameter types");
			param.PassedParameterExpression = _param_value_expression;
		}

		/// <summary>To string</summary>
		public override string								ToString()
		{
			return $"Function {Name} {(Description != null ? $"({Description})" : "")}";
		}

		#region Helpers

		private void										ResolveParameters(DataStorages _data_storages, IServiceProvider _service_provider)
		{
			foreach (var parameter in m_Parameters)
			{
				if (parameter.IsOutput)
					continue;
				var param_val_obj = ZScriptExpressionParser.ParseScriptExpression(parameter.PassedParameterExpression).EvaluateExpressionToValue(_data_storages, _service_provider);
				if (param_val_obj == null)
					throw new Exception($"Couldn't evaluate passed parameter {parameter.Name} expression {parameter.PassedParameterExpression} into value");
				//var tc = TypeDescriptor.GetConverter(parameter.ParameterType);
				//var param_val = tc.ConvertFrom(param_val_obj);
				if (param_val_obj.GetType() != parameter.ParameterType)
					throw new Exception($"Couldn't convert passed parameter {parameter.Name} expression {parameter.PassedParameterExpression} into value of type {parameter.ParameterType}");

				_data_storages.Store($"{(parameter.Name.StartsWith('$') ? "" : "$")}{parameter.Name}", param_val_obj);
			}
		}

		#endregion // Helpers

		#region XML Loading

		/// <summary>Parse Action from XmlElement</summary>
		public static FunctionAction						ParseFromXmlElement(FileInfo _src_file, XElement _x_element, List<IFunctionAction> _imported_functions, IServiceProvider _service_provider)
		{
			var name = _x_element.ReadNameAttr(_src_file, true);
			var desc = _x_element.ReadDescAttr(_src_file, false);
			var id = _x_element.ReadIdAttr(_src_file, false);
			
			// export
			List<string> exports = new List<string>();
			var export_el = _x_element.Element("Export");
			if (export_el != null)
			{
				foreach (var export_item in export_el.Elements("Item"))
				{
					exports.Add(export_item.Value.Trim());
				}
			}

			// parameter types
			List<FunctionParameter> parameters = new List<FunctionParameter>();
			var parameter_types_el = _x_element.Element("Parameters");
			if (parameter_types_el != null)
			{
				foreach (var x_param_el in parameter_types_el.Elements("Param"))
				{
					var pname = x_param_el.ReadAttr<string>("name", _src_file, true);
					var ptype_s = x_param_el.ReadAttr<string>("type", _src_file, true);
					var pis_output = x_param_el.ReadAttr<bool>("is_output", _src_file, false);
					var ptype = Type.GetType(ptype_s);

					if (string.IsNullOrWhiteSpace(pname))
						throw new ZTSXmlException($"Param must have name", _src_file.FullName, x_param_el.GetLineNumber(), x_param_el.GetLinePosition());
					if (ptype == null)
						throw new ZTSXmlException("Param must have target type", _src_file.FullName, x_param_el.GetLineNumber(), x_param_el.GetLinePosition());

					parameters.Add(new FunctionParameter()
					{
						Name = pname,
						ParameterType = ptype,
						IsOutput = pis_output
					});
				}
			}

			// actions
			List<IPlaylistAction> actions = new List<IPlaylistAction>();
			var x_actions_el = _x_element.Element("Actions");
			if (x_actions_el == null)
				throw new ZTSXmlException("Function must have Actions element containing all actions to take to perform function.", _src_file.FullName, _x_element.GetLineNumber(), _x_element.GetLinePosition());
			var test_action_factory = _service_provider.GetRequiredService<TestPlaylistActionFactory>();
			foreach (var x_action_element in x_actions_el.Elements())
			{
				var action = test_action_factory.LoadFromXmlElement(_src_file, x_action_element, _imported_functions, _service_provider);
				if (action != null)
					actions.Add(action);
				else
					throw new Exception($"Unknown action {x_action_element.Name}");
			}
			
			var func = new FunctionAction(id, name, desc, actions, exports, parameters);
			return func;
		}


		#endregion // XML Loading
	}
}
