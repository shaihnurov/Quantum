using Avalonia;
using System;

namespace Quantum;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
// Insert this line after the 'AppBuilder.Configure<App>()' line in your app's 'Program.BuildAvaloniaApp' method:
.RegisterActiproLicense("Future Actipro Customer", "AVA251-RVYDE-FBQQB-WHV64-6V3H");
}