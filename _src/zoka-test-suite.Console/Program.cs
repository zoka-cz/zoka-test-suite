using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Zoka.TestSuite.Abstraction;
using System.Reflection;
using System.Windows.Input;

namespace Zoka.TestSuite
{
	internal class Program
	{

		static int											Main(string[] args)
		{
			var root_command = ConfigureOptions();

			return root_command.InvokeAsync(args).Result;
		}

		private static Task<int>							RunSuite(FileInfo _suite_file, FileInfo? _config_file, FileInfo _log4net_config_file, DirectoryInfo[]? _plugin_directory)
		{
			var service_provider = ConfigureServiceProvider(_config_file, _log4net_config_file, _plugin_directory);
			var test_suite = Abstraction.TestSuite.FromXml(_suite_file, service_provider);
			var res = test_suite.Run(service_provider);
			return Task<int>.FromResult(res);
		}

		private static Task<int>							RunPlaylist(FileInfo _playlist_file, FileInfo? _config_file, FileInfo _log4net_config_file, DirectoryInfo[]? _plugin_directory)
		{
			ConfigureServiceProvider(_config_file, _log4net_config_file, _plugin_directory);
			return Task<int>.FromResult(0);
		}


		private static IServiceProvider						ConfigureServiceProvider(FileInfo? _config_file, FileInfo _log4net_config_file, DirectoryInfo[]? _plugin_directory)
		{
			var plugins = new List<IZokaTestSuitePlugin>(CollectPlugins(new DirectoryInfo("./plugins")));
			if (_plugin_directory != null)
			{
				foreach (var plug_dir in _plugin_directory)
					plugins.AddRange(CollectPlugins(plug_dir));
			}

			var service_collection = new ServiceCollection();
			CollectServices(service_collection, plugins, _config_file, _log4net_config_file);

			var service_provider = service_collection.BuildServiceProvider();
			ConfigureServices(service_provider, plugins);
			return service_provider;

		}

		// will collect services into passed IServiceCollection
		static void											CollectServices(IServiceCollection _services, IEnumerable<IZokaTestSuitePlugin> _plugins, FileInfo? _config_file, FileInfo _log4net_config_file)
		{
			_services.AddSingleton<IConfiguration>(BuildConfiguration(_config_file));
			if (!_log4net_config_file.Exists)
				throw new FileNotFoundException($"Log4Net configuration file {_log4net_config_file.Name} not found");
			_services.AddLogging((log_builder) =>
			{
				System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
				log_builder.SetMinimumLevel(LogLevel.Trace);
				log_builder.AddLog4Net(_log4net_config_file.FullName);
			});
			// testing
			_services.AddTransient<Abstraction.TestSuite>();
			_services.AddTransient<TestPlaylist>();
			_services.AddSingleton<TestPlaylistActionFactory>();

			// plugins
			foreach (var plugin in _plugins)
			{
				foreach (var collect_services_delegate in plugin.CollectServicesCallback)
				{
					collect_services_delegate(_services);
				}
			}
		}

		private static void									ConfigureServices(IServiceProvider _service_provider, IEnumerable<IZokaTestSuitePlugin> _plugins)
		{
			foreach (var plugin in _plugins)
			{
				foreach (var configure_services_delegate in plugin.ConfigureServicesCallback)
				{
					configure_services_delegate(_service_provider);
				}
			}
		}

		// Builds the IConfiguration for this app used inside the app
		private static IConfiguration						BuildConfiguration(FileInfo? _config_file)
		{
			// build configuration
			var config_builder = new ConfigurationBuilder();

			if (_config_file == null)
			{
				var config_file = new FileInfo("appsettings.json");
				if (config_file.Exists)
					config_builder.AddJsonFile(config_file.FullName);
			} 
			else if (_config_file.Exists)
				config_builder.AddJsonFile(_config_file.FullName);
			else
			{
				throw new FileNotFoundException($"Configuration file {_config_file} not found");
			}
			var config = config_builder.Build();
			return config;
		}

		private static IEnumerable<IZokaTestSuitePlugin>					CollectPlugins(DirectoryInfo? _plugin_directory)
		{
			if (_plugin_directory == null || !_plugin_directory.Exists)
				yield break;
			
			var dlls = _plugin_directory.EnumerateFiles("*.dll");
			foreach (var file_info in dlls)
			{
				var plugin_assembly = LoadPlugin(file_info.FullName);
				var plugins = CreatePlugin(plugin_assembly);
				foreach (var plugin in plugins)
				{
					yield return plugin;
				}
			}
		}

		static Assembly LoadPlugin(string relativePath)
		{
			// Navigate up to the solution root
			string root = Path.GetFullPath(Path.Combine(
				Path.GetDirectoryName(
					Path.GetDirectoryName(
						Path.GetDirectoryName(
							Path.GetDirectoryName(
								Path.GetDirectoryName(typeof(Program).Assembly.Location)!)!)!)!)!));

			string pluginLocation = Path.GetFullPath(Path.Combine(root, relativePath.Replace('\\', Path.DirectorySeparatorChar)));
			Console.WriteLine($"Loading commands from: {pluginLocation}");
			PluginLoadContext loadContext = new PluginLoadContext(pluginLocation);
			return loadContext.LoadFromAssemblyName(AssemblyName.GetAssemblyName(pluginLocation));
		}

		static IEnumerable<IZokaTestSuitePlugin>							CreatePlugin(Assembly assembly)
		{
			int count = 0;

			foreach (Type type in assembly.GetTypes())
			{
				if (typeof(IZokaTestSuitePlugin).IsAssignableFrom(type))
				{
					var result = Activator.CreateInstance(type) as IZokaTestSuitePlugin;
					if (result != null)
					{
						count++;
						yield return result;
					}
				}
			}

		}

		/// <summary>Will build options of the application</summary>
		private static RootCommand							ConfigureOptions()
		{
			var root_command = new RootCommand("Will perform integration testing of your application.");

			// common options
			var config_file_option = new Option<FileInfo?>(
				name: "--config",
				description: "Configuration file for providers used when running tests (if not specified, app tries to load appsettings.json, if exists)"
			);
			config_file_option.AddAlias("-c");
			var log4net_file_option = new Option<FileInfo>(
				name: "--log4netconfig",
				description:
				"Configuration file for log4net settings, if different from default log4net.config attached to this application",
				getDefaultValue: () => new FileInfo("log4net.config")
			);
			log4net_file_option.AddAlias("-l");
			var plugin_directory_option = new Option<DirectoryInfo[]?>(
				name: "--plugin-directory",
				description: "Additional directory, where the plugins are to be located. Program always search for plugins in directory ./plugins, by this parameter you may specify additional location"
				);
			plugin_directory_option.Arity = ArgumentArity.OneOrMore;
			plugin_directory_option.AllowMultipleArgumentsPerToken = true;
			plugin_directory_option.AddAlias("-pd");

			var run_suite_command = new Command("suite", "Runs the whole suite of tests playlist from the file.");
			var suite_file = new Option<FileInfo>(
				name: "--file",
				description: "Input file with suite of tests playlist");
			suite_file.AddAlias("-f");
			suite_file.IsRequired = true;
			run_suite_command.AddOption(suite_file);
			run_suite_command.SetHandler(RunSuite, suite_file, config_file_option, log4net_file_option, plugin_directory_option);

			var run_playlist_command = new Command("playlist", "Runs the tests playlist from the file.");
			var playlist_file = new Option<FileInfo>(
				name: "--file",
				description: "Input file with the tests playlist");
			playlist_file.AddAlias("-f");
			playlist_file.IsRequired = true;
			run_playlist_command.AddOption(playlist_file);
			run_playlist_command.SetHandler(RunPlaylist, playlist_file, config_file_option, log4net_file_option, plugin_directory_option);

			root_command.AddCommand(run_suite_command);
			root_command.AddCommand(run_playlist_command);
			root_command.AddGlobalOption(config_file_option);
			root_command.AddGlobalOption(log4net_file_option);
			root_command.AddGlobalOption(plugin_directory_option);
			return root_command;
		}
	}
}