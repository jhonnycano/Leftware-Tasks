using Leftware.Common;
using Leftware.Injection;
using Leftware.Tasks.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Runtime.Loader;

namespace Leftware.Tasks.UI;

public static class Initializer
{
    internal static Application Initialize()
    {
        var serviceProvider = ConfigureServices();
        var app = serviceProvider.GetService<Application>();
        return app;
    }

    private static ServiceProvider ConfigureServices()
    {
        IServiceCollection services = new ServiceCollection();

        services.AddOptions();

        PreloadAssemblies();

        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddJsonFile("appsettings.json");
        var configuration = configurationBuilder.Build();
        services.AddSingleton<IConfiguration>(configuration);

        ILogger logger = CreateLogger(services, configuration);

        var commonTaskTypeFinder = new CommonTaskTypeFinder(logger);
        var commonTaskLocator = new CommonTaskLocator(commonTaskTypeFinder);
        services.AddSingleton<CommonTaskTypeFinder>();
        services.AddSingleton<ICommonTaskLocator>(commonTaskLocator);

        var defaultProvider = new DefaultInjectionProvider(services);
        services.AddSingleton<IInjectionProvider>(defaultProvider);

        var finder = new ServiceFinder(defaultProvider, configuration, logger);
        finder.Find();

        var commonTaskInjector = new CommonTaskInjector(commonTaskLocator);
        commonTaskInjector.Inject(defaultProvider);

        // IMPORTANT! Register our application entry point
        services.AddTransient<Application>();

        var serviceProvider = services.BuildServiceProvider();
        var serviceLocator = serviceProvider.GetService<IServiceLocator>();
        finder.ServiceLocator = serviceLocator;
        finder.FindFactories();

        serviceProvider = services.BuildServiceProvider();
        return serviceProvider;
    }

    private static void PreloadAssemblies()
    {
        var currentAssembly = Assembly.GetExecutingAssembly();
        var path = Path.GetDirectoryName(currentAssembly.Location) ?? throw new InvalidOperationException("Assembly path not found");
        var implementationAssemblies = Directory.GetFiles(path, "Leftware.Tasks.*.dll");
        foreach (var assembly in implementationAssemblies)
        {
            AssemblyLoadContext.Default.LoadFromAssemblyPath(assembly);
        }
    }

    private static ILogger CreateLogger(IServiceCollection services, IConfigurationRoot configuration)
    {
        var logPath = configuration.GetValue("general:logPath", "");
        if (string.IsNullOrEmpty(logPath)) logPath = Environment.CurrentDirectory;
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
