using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Zoka.TestSuite.Abstraction.XMLHelpers
{
	/// <summary>Helpers to manipulate with XML parts inside TestSuite more easily</summary>
	public static class TestSuiteXmlHelpers
	{
		/// <summary>Will return the line info if available or -1</summary>
		public static int									GetLineNumber(this XElement _element)
		{
			return ((IXmlLineInfo)_element).HasLineInfo() ? ((IXmlLineInfo)_element).LineNumber : -1;
		}

		/// <summary>Will return the line position of the element or -1</summary>
		public static int									GetLinePosition(this XElement _element)
		{
			return ((IXmlLineInfo)_element).HasLineInfo() ? ((IXmlLineInfo)_element).LinePosition : -1;
		}

		/// <summary></summary>
		public static T										ReadAttr<T>(this XElement _element, string _attr_name, string _filename, bool _required)
		{
			var attr = _element.Attribute(_attr_name);
			if (attr == null)
			{
				if (_required)
					throw new ZTSXmlException($"Expected required attribute {_attr_name} on {_element.Name.LocalName} element", _filename, _element.GetLineNumber(), _element.GetLinePosition());
				return default(T);
			}
			var tc = new TypeConverter();
			var val = TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(attr.Value);
			return (T)val;
		}

		/// <summary>Will read and returns the name attribute from XElement (_name)</summary>
		public static string								ReadNameAttr(this XElement _element, string _filename, bool _required)
		{
			var name_attr = _element.Attribute("_name");
			if (name_attr != null)
			{
				return name_attr.Value;
			}
			if (_required)
				throw new ZTSXmlException($"Expected _name attribute on {_element.Name.LocalName} element", _filename, _element.GetLineNumber(), _element.GetLinePosition());

			return null;
		}
	}
}
