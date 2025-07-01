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

        public async Task<byte[]> GetPageImageAsync(string filePath, int pageNumber)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogError("El archivo PDF no existe: {FilePath}", filePath);
                    return null;
                }

                return await Task.Run(() =>
                {
                    using var document = PdfDocument.Load(filePath);
                    
                    if (pageNumber >= document.PageCount)
                    {
                        _logger.LogWarning("NÃºmero de pÃ¡gina {PageNumber} excede el total de pÃ¡ginas {TotalPages}", 
                            pageNumber, document.PageCount);
                        return null;
                    }

                    using var image = document.Render(pageNumber, 300, 300, PdfRenderFlags.CorrectFromDpi);
                    
                    using var memoryStream = new MemoryStream();
                    image.Save(memoryStream, ImageFormat.Png);
                    return memoryStream.ToArray();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener imagen de la pÃ¡gina {PageNumber} del archivo {FilePath}", 
                    pageNumber, filePath);
                return null;
            }
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
