using System.Text.Json;

namespace PdfReaderService.WindowsService.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ApiService> _logger;

        public ApiService(HttpClient httpClient, IConfiguration configuration, ILogger<ApiService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendDailyPageAsync()
        {
            try
            {
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"];
                var endpoint = $"{apiBaseUrl}/api/reading/send-daily-page";

                var response = await _httpClient.PostAsync(endpoint, null);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Respuesta API SendDailyPage: {Content}", content);
                    return true;
                }
                else
                {
                    _logger.LogError("Error en API SendDailyPage. Status: {StatusCode}", response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExcepciÃ³n al llamar SendDailyPageAsync");
                return false;
            }
        }

        public async Task<bool> SendNextPageAsync()
        {
            try
            {
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"];
                var endpoint = $"{apiBaseUrl}/api/reading/send-next-page";

                var response = await _httpClient.PostAsync(endpoint, null);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Respuesta API SendNextPage: {Content}", content);
                    return true;
                }
                else
                {
                    _logger.LogError("Error en API SendNextPage. Status: {StatusCode}", response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExcepciÃ³n al llamar SendNextPageAsync");
                return false;
            }
        }
    }
}
