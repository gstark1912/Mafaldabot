using System.Text.Json;

namespace PdfReaderService.Api.Services
{
    public class ImageUploadService : IImageUploadService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ImageUploadService> _logger;

        public ImageUploadService(HttpClient httpClient, ILogger<ImageUploadService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string> UploadImageAsync(byte[] imageData)
        {
            var content = new MultipartFormDataContent
    {
        { new StringContent("342cc82cca9a14a0d8e287fba8ddc84f"), "key" },
        { new ByteArrayContent(imageData), "image", "imagen.jpg" },
        { new StringContent("86400"), "expiration" } // 10 minutos
    };

            var response = await _httpClient.PostAsync("https://api.imgbb.com/1/upload", content);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var json = JsonDocument.Parse(result);
                var url = json.RootElement.GetProperty("data").GetProperty("url").GetString();
                _logger.LogInformation("Imagen subida correctamente: {Url}", url);
                return url;
            }
            else
            {
                _logger.LogError("Error al subir imagen: {StatusCode}, Detalle: {Detail}", response.StatusCode, result);
                return null;
            }
        }


    }
}