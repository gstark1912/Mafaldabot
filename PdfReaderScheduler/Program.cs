using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json;
using System.Text;

public class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Aplicación de Tareas Programadas ===");
        Console.WriteLine("Iniciando aplicación...");
        Console.WriteLine($"Hora de inicio: {DateTime.Now}");
        Console.WriteLine();

        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Configurar logging
                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Information);
                });

                // Registrar servicios
                services.AddSingleton<MongoDbService>();
                services.AddHttpClient<ApiService>();
                services.AddHostedService<ScheduledTaskService>();
            })
            .UseConsoleLifetime()
            .Build();

        try
        {
            await host.RunAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error crítico: {ex.Message}");
            Environment.Exit(1);
        }
    }
}