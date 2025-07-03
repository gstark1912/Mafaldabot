using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json;
using System.Text;

// Servicio principal de la tarea programada
public class ScheduledTaskService : BackgroundService
{
    private readonly MongoDbService _mongoDbService;
    private readonly ApiService _apiService;
    private readonly ILogger<ScheduledTaskService> _logger;
    private readonly string _apiBaseUrl;

    public ScheduledTaskService(
        MongoDbService mongoDbService,
        ApiService apiService,
        ILogger<ScheduledTaskService> logger)
    {
        _mongoDbService = mongoDbService;
        _apiService = apiService;
        _logger = logger;
        _apiBaseUrl = "http://192.168.0.18:5047"; // Cambia por tu URL base
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Servicio de tarea programada iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndExecuteTaskAsync();

                // Espera 12 horas (43200000 millisegundos)
                _logger.LogInformation("Próxima verificación en 12 horas");
                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Tarea programada cancelada");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en la tarea programada");
                // Espera 1 minuto antes de reintentar en caso de error
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

    private async Task CheckAndExecuteTaskAsync()
    {
        _logger.LogInformation("Verificando si es hora de ejecutar la tarea...");

        var readingState = await _mongoDbService.GetReadingStateAsync();
        if (readingState == null)
        {
            _logger.LogWarning("No se pudo obtener el estado de lectura");
            return;
        }

        var currentTime = DateTime.Now;
        _logger.LogInformation("Hora actual: {CurrentTime}", currentTime.ToString("HH:mm:ss"));

        // Parsear la hora de ejecución
        if (!TimeSpan.TryParse(readingState.RunTimeOfDay, out var runTime))
        {
            _logger.LogError("No se pudo parsear la hora de ejecución: {RunTimeOfDay}", readingState.RunTimeOfDay);
            return;
        }

        var todayRunTime = DateTime.Today.Add(runTime);
        _logger.LogInformation("Hora programada para hoy: {TodayRunTime}", todayRunTime.ToString("HH:mm:ss"));

        // Verificar si ya pasó la hora programada y si no se ejecutó hoy
        bool shouldExecute = currentTime >= todayRunTime &&
                           readingState.LastRunDateTime.Date < DateTime.Today;

        if (shouldExecute)
        {
            _logger.LogInformation("Es hora de ejecutar la tarea. Enviando solicitud al API...");

            bool success = await _apiService.SendDailyPageAsync(_apiBaseUrl);

            if (success)
            {
                _logger.LogInformation("Solicitud enviada exitosamente. Actualizando fecha de última ejecución...");
                await _mongoDbService.UpdateLastRunDateTimeAsync(readingState.Id, currentTime);
            }
            else
            {
                _logger.LogError("Error al enviar solicitud al API");
            }
        }
        else
        {
            if (readingState.LastRunDateTime.Date >= DateTime.Today)
            {
                _logger.LogInformation("La tarea ya se ejecutó hoy ({LastRun})", readingState.LastRunDateTime);
            }
            else
            {
                _logger.LogInformation("Aún no es hora de ejecutar la tarea");
            }
        }
    }
}