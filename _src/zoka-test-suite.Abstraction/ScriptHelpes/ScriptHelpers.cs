using Stubble.Core.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Jint;
using Jint.Native;
using Stubble.Core.Parser.TokenParsers;

namespace Zoka.TestSuite.Abstraction.ScriptHelpes
{
	/// <summary></summary>
	public class ScriptHelper
	{
		private readonly DataStorages						m_DataStorages;
		private readonly IServiceProvider					m_ServiceProvider;


		/// <summary></summary>
		public ScriptHelper(DataStorages _data_storages, IServiceProvider _service_provider)
		{
			m_DataStorages = _data_storages;
			m_ServiceProvider = _service_provider;
		}

		/// <summary></summary>
		public string										EvaluateStringReplacements(string _template)
		{
			var stubble = new StubbleBuilder()
				.Configure(s =>
				{
					s.AddValueGetter(typeof(ScriptHelper), ValueGetter);
					s.ConfigureParserPipeline(ps =>
					{
						ps.AddBefore<InterpolationTagParser>(new JsTagParser());
					});
					s.TokenRenderers.AddIfNotAlready<JsTokenRenderer>();
				})
				.Build();

			return stubble.Render(_template, this);
		}

		private static object ValueGetter(object _value, string _key, bool _ignorecase)
		{
			var ctx = _value as ScriptHelper;
			return ctx?.m_DataStorages.GetObjectFromDataStorage(_key, true) ?? null!;
		}

		/// <summary>Will run JsExpression, where one may use the $data_storage_variables as part of the script</summary>
		public string										RunJsExpressionIntoString(string _js_expression)
		{
			var eng = new Engine();

			foreach (var data_storage in m_DataStorages.ToArray())
			{
				foreach (var key in data_storage.Keys)
				{
					eng.SetValue(key, data_storage[key]);
				}
			}

			var ret_val = eng.Evaluate(_js_expression);
			return ret_val.ToString();
		}

		/// <summary>Will run JsExpression, where one may use the $data_storage_variables as part of the script and returns the JsValue</summary>
		public JsValue										RunJsExpression(string _js_expression)
		{
			var eng = new Engine();

			foreach (var data_storage in m_DataStorages.ToArray())
			{
				foreach (var key in data_storage.Keys)
				{
					eng.SetValue(key, data_storage[key]);
				}
			}

			var ret_val = eng.Evaluate(_js_expression);
			return ret_val;
		}
	}
}
