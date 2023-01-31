using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Zoka.TestSuite
{
	internal class Program
	{

		static int											Main(string[] args)
		{
			var root_command = ConfigureOptions();

			return root_command.InvokeAsync(args).Result;
		}

		private static Task<int>							RunSuite(FileInfo _suite_file, FileInfo _config_file, FileInfo _log4net_config_file)
		{
			var service_provider = ConfigureServiceProvider(_config_file, _log4net_config_file);
			var test_suite = Abstraction.TestSuite.FromXml(_suite_file, service_provider);
			var res = test_suite.Run(service_provider);
			return Task<int>.FromResult(res);
		}

		private static Task<int>							RunPlaylist(FileInfo _playlist_file, FileInfo _config_file, FileInfo _log4net_config_file)
		{
			ConfigureServiceProvider(_config_file, _log4net_config_file);
			return Task<int>.FromResult(0);
		}


		private static IServiceProvider						ConfigureServiceProvider(FileInfo _config_file, FileInfo _log4net_config_file)
		{
			var service_collection = new ServiceCollection();
			CollectServices(service_collection, _config_file, _log4net_config_file);

			var service_provider = service_collection.BuildServiceProvider();
			ConfigureServices(service_provider);
			return service_provider;

		}

		// will collect services into passed IServiceCollection
		static void											CollectServices(IServiceCollection _services, FileInfo _config_file, FileInfo _log4net_config_file)
		{
			_services.AddSingleton<IConfiguration>(BuildConfiguration(_config_file));
			if (!_log4net_config_file.Exists)
				throw new FileNotFoundException($"Log4Net configuration file {_log4net_config_file.Name} not found");
			_services.AddLogging((log_builder) =>
			{
				log_builder.SetMinimumLevel(LogLevel.Trace);
				log_builder.AddLog4Net(_log4net_config_file.FullName);
			});
			//_services.AddTesting();
		}

		private static void									ConfigureServices(IServiceProvider _service_provider)
		{

		}

		// Builds the IConfiguration for this app used inside the app
		private static IConfiguration						BuildConfiguration(FileInfo _config_file)
		{
			// build configuration
			var config_builder = new ConfigurationBuilder();
			if (_config_file.Exists)
				config_builder.AddJsonFile(_config_file.FullName);
			else
			{
				throw new FileNotFoundException($"Configuration file {_config_file} not found");
			}
			var config = config_builder.Build();
			return config;
		}


		/// <summary>Will build options of the application</summary>
		private static RootCommand							ConfigureOptions()
		{
			var root_command = new RootCommand("Will perform integration testing of your application.");

			// common options
			var config_file_option = new Option<FileInfo>(
				name: "--config",
				description: "Configuration file for providers used when running tests",
				getDefaultValue: () => new FileInfo("appsettings.json")
			);
			config_file_option.AddAlias("-c");
			var log4net_file_option = new Option<FileInfo>(
				name: "--log4netconfig",
				description:
				"Configuration file for log4net settings, if different from default log4net.config attached to this application",
				getDefaultValue: () => new FileInfo("log4net.config")
			);
			log4net_file_option.AddAlias("-l");

			var run_suite_command = new Command("suite", "Runs the whole suite of tests playlist from the file.");
			var suite_file = new Option<FileInfo>(
				name: "--file",
				description: "Input file with suite of tests playlist");
			suite_file.AddAlias("-f");
			suite_file.IsRequired = true;
			run_suite_command.AddOption(suite_file);
			run_suite_command.AddOption(config_file_option);
			run_suite_command.AddOption(log4net_file_option);
			run_suite_command.SetHandler(RunSuite, suite_file, config_file_option, log4net_file_option);

			var run_playlist_command = new Command("playlist", "Runs the tests playlist from the file.");
			var playlist_file = new Option<FileInfo>(
				name: "--file",
				description: "Input file with the tests playlist");
			playlist_file.AddAlias("-f");
			playlist_file.IsRequired = true;
			run_playlist_command.AddOption(playlist_file);
			run_playlist_command.AddOption(config_file_option);
			run_playlist_command.AddOption(log4net_file_option);
			run_playlist_command.SetHandler(RunPlaylist, playlist_file, config_file_option, log4net_file_option);

			root_command.AddCommand(run_suite_command);
			root_command.AddCommand(run_playlist_command);
			return root_command;
		}
	}
}