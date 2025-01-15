using Microsoft.EntityFrameworkCore;
using Serilog;
using Server.Hub;
using Server.Model;

namespace Server;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
        
        builder.Host.UseSerilog();
        
        builder.Services.AddDbContext<ApplicationContextDB>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
        
        builder.Services.AddSignalR();
        builder.Services.AddScoped<AuthHub>();
        builder.Services.AddScoped<RegisterHub>();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
        });
        
        var app = builder.Build();
        
        app.UseHttpsRedirection();
        app.UseCors("AllowAll");
        app.UseHsts();
        
        Log.Information("Starting web server");
        
        app.MapHub<AuthHub>("/auth");
        app.MapHub<RegisterHub>("/register");
        
        app.MapGet("/", () => "Successful connection");

        try
        {
            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Unhandled exception");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}