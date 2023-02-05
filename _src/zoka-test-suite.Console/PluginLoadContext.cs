using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace Zoka.TestSuite
{
	class PluginLoadContext : AssemblyLoadContext
	{
		private AssemblyDependencyResolver					m_Resolver;

		public PluginLoadContext(string _plugin_path)
		{
			m_Resolver = new AssemblyDependencyResolver(_plugin_path);
		}

		protected override Assembly?						Load(AssemblyName _assembly_name)
		{
			var assemblyPath = m_Resolver.ResolveAssemblyToPath(_assembly_name);
			if (assemblyPath != null)
			{
				return LoadFromAssemblyPath(assemblyPath);
			}

			return null;
		}

		protected override IntPtr							LoadUnmanagedDll(string _unmanaged_dll_name)
		{
			var libraryPath = m_Resolver.ResolveUnmanagedDllToPath(_unmanaged_dll_name);
			if (libraryPath != null)
			{
				return LoadUnmanagedDllFromPath(libraryPath);
			}

			return IntPtr.Zero;
		}
	}
}
