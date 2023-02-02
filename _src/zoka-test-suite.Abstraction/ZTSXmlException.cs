using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace Zoka.TestSuite.Abstraction
{
	/// <summary>XmlException enhanced by the file, in which the error occured</summary>
	public class ZTSXmlException : XmlException
	{
		/// <summary>Constructor</summary>
		public ZTSXmlException(string _message) : 
			base(_message) { }
		/// <summary>Constructor</summary>
		public ZTSXmlException(string _message, string _filename, int _line_number, int _line_position)
			: base($"{_message} at {_filename}", null, _line_number, _line_position) {}
		/// <summary>Constructor</summary>
		public ZTSXmlException(string _message, Exception _inner_exception, string _filename, int _line_number, int _line_position)
			: base($"{_message} at {_filename}", _inner_exception, _line_number, _line_position) { }
	}
}
