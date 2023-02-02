using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Zoka.TestSuite.Abstraction
{
	/// <summary>Delegate which is used for collecting services during building up service provider (DependencyInjection)</summary>
	public delegate IServiceCollection						CollectServicesDelegate(IServiceCollection _service_collection);

	/// <summary>Delegate which is used for configuring services once the service collection has been finished and services are about to be configured before fisrt use if necessary (DependenciInjection)</summary>
	public delegate IServiceProvider						ConfigureServicesDelegate(IServiceProvider _service_provider);

	/// <summary>Delegate which loads the IPlaylist action from the Xml element</summary>
	public delegate IPlaylistAction							CreateFromXmlElementDelegate(FileInfo _src_file, XElement _element, IServiceProvider _service_provider);
}
