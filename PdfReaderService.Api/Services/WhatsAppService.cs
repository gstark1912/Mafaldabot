using System.Text;
using System.Text.Json;

namespace PdfReaderService.Api.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<WhatsAppService> _logger;

        public WhatsAppService(HttpClient httpClient, IConfiguration configuration, ILogger<WhatsAppService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendImageAsync(byte[] imageData, string caption)
        {
            try
            {
                var whatsappEndpoint = _configuration["WhatsApp:SendImageEndpoint"];
                var recipientNumber = _configuration["WhatsApp:RecipientNumber"];
                var session = _configuration["WhatsApp:Session"] ?? "default";

                var payload = new
                {
                    chatId = recipientNumber,
                    file = new
                    {
                        mimetype = "image/jpeg",
                        filename = "imagen.jpg",
                        url = "https://wow.zamimg.com/uploads/screenshots/small/629956.jpg"
                    },
                    caption = caption,
                    session = session
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(whatsappEndpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Imagen enviada correctamente por WhatsApp");
                    return true;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al enviar imagen por WhatsApp. Status: {StatusCode}, Detalle: {Detalle}", response.StatusCode, error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExcepciÃ³n al enviar imagen por WhatsApp");
                return false;
            }
        }

        public async Task<bool> SendTextAsync(string message)
        {
            try
            {
                var whatsappEndpoint = _configuration["WhatsApp:SendTextEndpoint"];
                var recipientNumber = _configuration["WhatsApp:RecipientNumber"];
                var session = _configuration["WhatsApp:Session"] ?? "default";

                if (!await IsSessionStartedAsync())
                    return false;

                var payload = new
                {
                    chatId = recipientNumber,
                    reply_to = (string)null,
                    text = message,
                    linkPreview = true,
                    linkPreviewHighQuality = false,
                    session = session
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(whatsappEndpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Mensaje de texto enviado correctamente por WhatsApp");
                    return true;
                }
                else
                {
                    _logger.LogError("Error al enviar mensaje de texto por WhatsApp. Status: {StatusCode}", response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExcepciÃ³n al enviar mensaje de texto por WhatsApp");
                return false;
            }
        }

        public async Task<bool> IsSessionStartedAsync()
        {
            try
            {
                var baseUrl = _configuration["WhatsApp:BaseUrl"];
                var sessionName = _configuration["WhatsApp:Session"] ?? "default";

                var statusUrl = $"{baseUrl}/sessions/{sessionName}";
                var startUrl = $"{statusUrl}/start";

                var statusResponse = await _httpClient.GetAsync(statusUrl);
                if (!statusResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("Error al consultar el estado de la sesión. Status: {StatusCode}", statusResponse.StatusCode);
                    return false;
                }

                using var stream = await statusResponse.Content.ReadAsStreamAsync();
                var json = await JsonDocument.ParseAsync(stream);
                var status = json.RootElement.GetProperty("status").GetString();

                if (status == "STOPPED")
                {
                    _logger.LogInformation("Sesión está detenida. Intentando iniciar...");

                    var startResponse = await _httpClient.PostAsync(startUrl, new StringContent(""));
                    if (startResponse.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Sesión iniciándose correctamente.");
                        return true;
                    }
                    else
                    {
                        _logger.LogError("Error al iniciar la sesión. Status: {StatusCode}", startResponse.StatusCode);
                        return false;
                    }
                }

                _logger.LogInformation("Sesión ya está activa. Estado: {Status}", status);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción al verificar o iniciar la sesión de WhatsApp");
                return false;
            }
        }

    }
}
