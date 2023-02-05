using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
		public static T										ReadAttr<T>(this XElement _element, string _attr_name, FileInfo _src_file, bool _required)
		{
			var attr = _element.Attribute(_attr_name);
			if (attr == null)
			{
				if (_required)
					throw new ZTSXmlException($"Expected required attribute {_attr_name} on {_element.Name.LocalName} element", _src_file.Name, _element.GetLineNumber(), _element.GetLinePosition());
				return default(T);
			}
			var tc = new TypeConverter();
			var val = TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(attr.Value);
			return (T)val;
		}

		/// <summary>Will read and returns the name attribute from XElement (_name)</summary>
		public static string								ReadNameAttr(this XElement _element, FileInfo _src_file, bool _required) 
			=> _element.ReadAttr<string>("_name", _src_file, _required);

		/// <summary>Will read the description attribute from XElement (_description)</summary>
		public static string								ReadDescAttr(this XElement _element, FileInfo _src_file, bool _required)
			=> _element.ReadAttr<string>("_description", _src_file, _required);

		/// <summary>Will read the Id attribute from XElement (_id)</summary>
		public static string ReadIdAttr(this XElement _element, FileInfo _src_file, bool _required)
			=> _element.ReadAttr<string>("_id", _src_file, _required);

	}
}
