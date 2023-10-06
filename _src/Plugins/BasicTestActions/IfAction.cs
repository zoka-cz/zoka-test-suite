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
	internal class IfAction : IPlaylistAction
	{
		/// <summary>XML element name</summary>
		public const string									ACTION_TYPE_NAME = "If";

		/// <summary>Name of the action</summary>
		public string										Name { get; }
		/// <summary>Id of the action</summary>
		public string										Id { get; } 
		/// <summary>Description of the action</summary>
		public string										Description { get; }

		private string										m_ConditionExpr;

		private List<IPlaylistAction>						m_SubActions;

		/// <summary>Constructor</summary>
		private IfAction(string _name, string _id, string _description, string _condition_expr, List<IPlaylistAction> _sub_actions)
		{
			Name = _name;
			Id = _id;
			Description = _description;
			m_ConditionExpr = _condition_expr;
			m_SubActions = _sub_actions;
		}

		/// <inheritdoc />
		public EPlaylistActionResultInstruction PerformAction(DataStorages _data_storages, IServiceProvider _service_provider)
		{
			var script_helper = new ScriptHelper(_data_storages, _service_provider);
			var logger = _service_provider.GetRequiredService<ILogger<IfAction>>();

			var condition_res = script_helper.RunJsExpression(m_ConditionExpr);
			if (!condition_res.IsBoolean())
				throw new Exception("Condition in JS has not evaluated into boolean.");

			var res = condition_res.AsBoolean();
			logger.LogInformation($"Condition expression (\"{m_ConditionExpr}\") evaluated as {res}");

			if (res)
			{
				var should_break = false;
				foreach (var action in m_SubActions)
				{
					var ret = action.PerformAction(_data_storages, _service_provider);
					if (ret == EPlaylistActionResultInstruction.Fail)
						return ret;
					if (ret == EPlaylistActionResultInstruction.BreakLoop || ret == EPlaylistActionResultInstruction.ContinueLoop)
					{
						should_break = ret == EPlaylistActionResultInstruction.BreakLoop;
						break;
					}
				}

				if (should_break)
					return EPlaylistActionResultInstruction.BreakLoop;
			}

			return EPlaylistActionResultInstruction.NoInstruction;
		}

		/// <summary>To string</summary>
		public override string								ToString()
		{
			return $"Function {Name} {(Description != null ? $"({Description})" : "")}";
		}

		#region XML Loading

		/// <summary>Parse Action from XmlElement</summary>
		public static IfAction								ParseFromXmlElement(FileInfo _src_file, XElement _x_element, List<IFunctionAction> _imported_functions, IServiceProvider _service_provider)
		{
			var name = _x_element.ReadNameAttr(_src_file, false);
			var desc = _x_element.ReadDescAttr(_src_file, false);
			var id = _x_element.ReadIdAttr(_src_file, false);

			var condition_expr = _x_element.ReadAttr<string>("condition", _src_file, true);

			var sub_actions = new List<IPlaylistAction>();
			var test_action_factory = _service_provider.GetRequiredService<TestPlaylistActionFactory>();

			foreach (var x_sub_action in _x_element.Elements())
			{
				var action = test_action_factory.LoadFromXmlElement(_src_file, x_sub_action, _imported_functions, _service_provider);
				if (action != null)
					sub_actions.Add(action);
				else
				{
					throw new Exception($"Unknown action {x_sub_action.Name}");
				}
			}

			var func = new IfAction(name, id, desc, condition_expr, sub_actions);
			return func;
		}


		#endregion // XML Loading

	}
}
