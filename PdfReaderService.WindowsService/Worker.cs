using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using PdfReaderService.WindowsService.Models;
using PdfReaderService.WindowsService.Services;

namespace PdfReaderService.WindowsService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Servicio PDF Reader iniciado");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var database = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
                    var apiService = scope.ServiceProvider.GetRequiredService<IApiService>();

                    await CheckAndSendDailyPageAsync(database, apiService);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en el servicio PDF Reader");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task CheckAndSendDailyPageAsync(IMongoDatabase database, IApiService apiService)
        {
            try
            {
                var collection = database.GetCollection<ReadingState>("ReadingState");
                var readingState = await collection.Find(_ => true).FirstOrDefaultAsync();

                if (readingState == null)
                {
                    _logger.LogWarning("No se encontrÃ³ configuraciÃ³n de ReadingState");
                    return;
                }

                var now = DateTime.Now;
                var today = DateOnly.FromDateTime(now);
                var currentTime = TimeOnly.FromDateTime(now);
                var lastRunDate = DateOnly.FromDateTime(readingState.LastRunDateTime);

                bool shouldRun = false;

                if (lastRunDate < today && currentTime >= readingState.RunTimeOfDay)
                {
                    shouldRun = true;
                }
                else if (lastRunDate < today.AddDays(-1))
                {
                    shouldRun = true;
                }

                if (shouldRun)
                {
                    _logger.LogInformation("Enviando pÃ¡gina diaria...");
                    var success = await apiService.SendDailyPageAsync();
                    
                    if (success)
                    {
                        _logger.LogInformation("PÃ¡gina diaria enviada correctamente");
                    }
                    else
                    {
                        _logger.LogError("Error al enviar pÃ¡gina diaria");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en CheckAndSendDailyPageAsync");
            }
        }
    }
}
