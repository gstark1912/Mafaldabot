using PdfiumViewer;
using System.Drawing;
using System.Drawing.Imaging;

namespace PdfReaderService.Api.Services
{
    public class PdfService : IPdfService
    {
        private readonly ILogger<PdfService> _logger;

        public PdfService(ILogger<PdfService> logger)
        {
            _logger = logger;
        }

        public byte[] GetPageImageAsync(string filePath, int pageNumber)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogError("El archivo PDF no existe: {FilePath}", filePath);
                    return null;
                }

                return RenderPageToImage(filePath, pageNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener imagen de la pÃ¡gina {PageNumber} del archivo {FilePath}",
                    pageNumber, filePath);
                return null;
            }
        }

        private byte[] RenderPageToImage(string filePath, int pageNumber)
        {
            using var document = PdfDocument.Load(filePath);
            if (pageNumber < 0 || pageNumber >= document.PageCount)
            {
                _logger.LogError("NÃºmero de pÃ¡gina fuera de rango: {PageNumber}", pageNumber);
                return null;
            }

            using var image = document.Render(pageNumber, 300, 300, true);
            using var ms = new MemoryStream();
            image.Save(ms, ImageFormat.Png);
            return ms.ToArray();
        }

        public async Task<int> GetTotalPagesAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogError("El archivo PDF no existe: {FilePath}", filePath);
                    return 0;
                }

                return await Task.Run(() =>
                {
                    using var document = PdfDocument.Load(filePath);
                    return document.PageCount;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener total de pÃ¡ginas del archivo {FilePath}", filePath);
                return 0;
            }
        }
    }
}
