using Avalonia;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.IO;
using uchat.Services;
using uchat.ViewModels;

namespace uchat;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        App.Services = services.BuildServiceProvider();

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    private static void ConfigureServices(ServiceCollection services)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
            
        
        services.AddSingleton<IConfiguration>(configuration);

        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IUserSession, UserSession>();
        services.AddSingleton<IServerClient, ServerClient>();

        services.AddTransient<LoaderViewModel>();
        services.AddTransient<LoginWindowViewModel>();
        services.AddTransient<RegistrationWindowViewModel>();
        services.AddTransient<MainWindowViewModel>();
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
