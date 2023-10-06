using System;
using System.Collections.Generic;
using System.Text;
using Stubble.Core.Tokens;

namespace Zoka.TestSuite.Abstraction.ScriptHelpes
{
	internal class JsToken : InlineToken<JsToken>
	{
		/// <inheritdoc/>
		public override bool Equals(JsToken other)
		{
			if (other == null)
			{
				return false;
			}

			return other.Content.Equals(Content) &&
			       other.TagStartPosition == TagStartPosition &&
			       other.TagEndPosition == TagEndPosition &&
			       other.ContentStartPosition == ContentStartPosition &&
			       other.ContentEndPosition == ContentEndPosition &&
			       other.IsClosed == IsClosed;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is JsToken a && Equals(a);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			unchecked
			{
				var hash = 27;
				hash = (13 * hash) + TagStartPosition.GetHashCode();
				hash = (13 * hash) + TagEndPosition.GetHashCode();
				hash = (13 * hash) + ContentStartPosition.GetHashCode();
				hash = (13 * hash) + ContentEndPosition.GetHashCode();
				hash = (13 * hash) + Content.GetHashCode();
				hash = (13 * hash) + IsClosed.GetHashCode();
				return hash;
			}
		}
	}
}
