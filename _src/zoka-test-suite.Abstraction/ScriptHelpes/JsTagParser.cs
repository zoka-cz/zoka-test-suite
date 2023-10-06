using Stubble.Core.Exceptions;
using Stubble.Core.Imported;
using Stubble.Core.Parser.Interfaces;
using Stubble.Core.Parser;
using System;
using System.Collections.Generic;
using System.Text;

namespace Zoka.TestSuite.Abstraction.ScriptHelpes
{
	internal class JsTagParser : InlineParser
	{
		public override bool Match(Processor processor, ref StringSlice slice)
		{
			var tagStart = slice.Start - processor.CurrentTags.StartTag.Length;
			var index = slice.Start;

			while (slice[index].IsWhitespace())
			{
				index++;
			}

			if (!slice.Match("js:"))
			{
				return false;
			}

			index += 3;

			while (slice[index].IsWhitespace())
			{
				index++;
			}

			slice.Start = index;
			var startIndex = index;

			var endTag = processor.CurrentTags.EndTag;
			while (!slice.IsEmpty && !slice.Match(endTag))
			{
				slice.NextChar();
			}

			var content = new StringSlice(slice.Text, startIndex, slice.Start - 1);
			content.TrimEnd();
			var contentEnd = content.End + 1;

			var tag = new JsToken()
			{
				TagStartPosition = tagStart,
				ContentStartPosition = startIndex,
				IsClosed = true
			};

			if (!slice.Match(endTag))
			{
				throw new StubbleException($"Unclosed Tag at {slice.Start.ToString()}");
			}

			tag.ContentEndPosition = contentEnd;
			tag.TagEndPosition = slice.Start + endTag.Length;
			slice.Start += endTag.Length;

			processor.CurrentToken = tag;
			processor.HasSeenNonSpaceOnLine = true;

			return true;
		}
	}
}
