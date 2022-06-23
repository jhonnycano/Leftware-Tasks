using Leftware.Injection;
using Leftware.Tasks.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace Leftware.Tasks.UI;

public static class Initializer
{
    internal static Application Initialize()
    {
        var serviceProvider = ConfigureServices();
        var app = serviceProvider.GetService<Application>() ?? throw new InvalidOperationException("Application object not found");
        return app;
    }

    private static ServiceProvider ConfigureServices()
    {
        IServiceCollection services = new ServiceCollection();

        services.AddOptions();

        SetupNewtonsoft();

        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddJsonFile("appsettings.json");
        var configuration = configurationBuilder.Build();
        services.AddSingleton<IConfiguration>(configuration);

        var logger = CreateLogger(services, configuration);

        var injectionWorkers = new[] { new CommonTaskInjectionWorker() };
        services.AddLeftwareInjection("Leftware.Tasks", configuration, logger, injectionWorkers);

        // IMPORTANT! Register our application entry point
        services.AddTransient<Application>();
        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider;
    }

    private static void SetupNewtonsoft()
    {
        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            }
        };

        settings.Converters.Add(new StringEnumConverter());
        JsonConvert.DefaultSettings = () => settings;
    }
    
    private static ILogger CreateLogger(IServiceCollection services, IConfigurationRoot configuration)
    {
        var logPath = configuration.GetValue("general:logPath", "");
        var globalAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appDataPath = Path.Combine(globalAppDataPath, "Leftware", "Logs");
        if (string.IsNullOrEmpty(logPath)) logPath = appDataPath;

        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyFolder = Path.GetDirectoryName(assemblyLocation);
        logPath = logPath.Replace("{{exePath}}", assemblyFolder);
        var logFolderPath = Path.GetFullPath(logPath);
        if (!Directory.Exists(logFolderPath)) Directory.CreateDirectory(logFolderPath);

        var logFilePath = Path.Combine(logFolderPath, "logs-{Date}.log");

        var loggerFactory = LoggerFactory.Create(builder => { });

        loggerFactory.AddFile(logFilePath);
        services.AddSingleton(sp => loggerFactory.CreateLogger("execution"));
        var logger = loggerFactory.CreateLogger("initialize");
        return logger;
    }
}
