using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json;
using System.Text;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiService> _logger;

    public ApiService(HttpClient httpClient, ILogger<ApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> SendDailyPageAsync(string baseUrl)
    {
        try
        {
            var url = $"{baseUrl}/api/Reading/send-daily-page";
            _logger.LogInformation("Enviando solicitud POST a {Url}", url);

            var response = await _httpClient.PostAsync(url, null);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Respuesta exitosa del API: {StatusCode}", response.StatusCode);
                _logger.LogDebug("Contenido de respuesta: {Content}", content);
                return true;
            }
            else
            {
                _logger.LogWarning("Error en respuesta del API: {StatusCode} - {ReasonPhrase}",
                    response.StatusCode, response.ReasonPhrase);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar solicitud al API");
            return false;
        }
    }
}