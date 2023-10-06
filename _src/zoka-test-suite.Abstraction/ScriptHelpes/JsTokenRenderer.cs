using Stubble.Core.Contexts;
using Stubble.Core.Renderers.StringRenderer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace Zoka.TestSuite.Abstraction.ScriptHelpes
{
	internal class JsTokenRenderer : StringObjectRenderer<JsToken>
	{
		/// <summary>
		/// Renders the value to string using a locale.
		/// </summary>
		/// <param name="obj">The object to convert</param>
		/// <param name="culture">The culture to use</param>
		/// <returns>The object stringified into the locale</returns>
		protected static string								ConvertToStringInCulture(object obj, CultureInfo culture)
		{
			if (obj is null || obj is string)
			{
				return obj as string;
			}

			return Convert.ToString(obj, culture);
		}


		protected override void Write(StringRender renderer, JsToken obj, Context context)
		{
			if (context.View is ScriptHelper script_helper)
			{
				var ret_string = script_helper.RunJsExpressionIntoString(obj.Content.ToString());

				if (obj.Indent > 0)
				{
					renderer.Write(' ', obj.Indent);
				}

				renderer.Write(ConvertToStringInCulture(ret_string, context.RenderSettings.CultureInfo));
				return;
			}
			throw new NotImplementedException();
		}

		protected override Task WriteAsync(StringRender renderer, JsToken obj, Context context)
		{
			throw new NotImplementedException();
		}
	}
}
