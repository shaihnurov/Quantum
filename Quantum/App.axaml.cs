using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using Quantum.ViewModels;
using Quantum.Views;
using Serilog;
using System.Threading.Tasks;
using Quantum.Service;

namespace Quantum;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs/application.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        Log.Information("���������� ��������");
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };

            desktop.ShutdownRequested += (_, _) => OnAppShutdown();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnAppShutdown()
    {
        // ��������� ������ � ������������ � ��������� ���� "��������� ����"
        Task.Run(async () =>
        {
            var userData = await UserDataStorage.GetUserData();
            if (userData != null && !userData.IsRememberMe)
            {
                UserDataStorage.DeleteUserData();
            }
        }).Wait();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}