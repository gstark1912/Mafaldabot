using MongoDB.Driver;
using PdfReaderService.Api.Models;

namespace PdfReaderService.Api.Services
{
    public class ReadingService : IReadingService
    {
        private readonly IMongoCollection<ReadingState> _readingStateCollection;
        private readonly IPdfService _pdfService;
        private readonly IWhatsAppService _whatsAppService;
        private readonly ILogger<ReadingService> _logger;

        public ReadingService(
            IMongoDatabase database,
            IPdfService pdfService,
            IWhatsAppService whatsAppService,
            ILogger<ReadingService> logger)
        {
            _readingStateCollection = database.GetCollection<ReadingState>("ReadingState");
            _pdfService = pdfService;
            _whatsAppService = whatsAppService;
            _logger = logger;
        }

        public async Task<bool> SendDailyPageAsync()
        {
            try
            {
                var readingState = await GetReadingStateAsync();
                if (readingState == null)
                {
                    _logger.LogWarning("No se encontrÃ³ configuraciÃ³n de lectura");
                    return false;
                }

                if (readingState.CurrentPage > readingState.EndPage)
                {
                    _logger.LogInformation("Lectura completada. No hay mÃ¡s pÃ¡ginas para enviar.");
                    return false;
                }

                var pageImage = await _pdfService.GetPageImageAsync(readingState.FilePath, readingState.CurrentPage);
                if (pageImage == null)
                {
                    _logger.LogError("No se pudo obtener la imagen de la pÃ¡gina {PageNumber}", readingState.CurrentPage);
                    return false;
                }

                var message = $"ðŸ“– PÃ¡gina {readingState.CurrentPage + 1} de tu lectura diaria";
                var sent = await _whatsAppService.SendImageAsync(pageImage, message);

                if (sent)
                {
                    readingState.CurrentPage++;
                    readingState.LastRunDateTime = DateTime.Now;
                    await UpdateReadingStateAsync(readingState);
                    
                    _logger.LogInformation("PÃ¡gina {PageNumber} enviada correctamente", readingState.CurrentPage);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en SendDailyPageAsync");
                return false;
            }
        }

        public async Task<bool> SendNextPageAsync()
        {
            try
            {
                var readingState = await GetReadingStateAsync();
                if (readingState == null)
                {
                    _logger.LogWarning("No se encontrÃ³ configuraciÃ³n de lectura");
                    return false;
                }

                if (readingState.CurrentPage > readingState.EndPage)
                {
                    await _whatsAppService.SendTextAsync("ðŸ“š Â¡Has completado toda la lectura! ðŸŽ‰");
                    return true;
                }

                var pageImage = await _pdfService.GetPageImageAsync(readingState.FilePath, readingState.CurrentPage);
                if (pageImage == null)
                {
                    _logger.LogError("No se pudo obtener la imagen de la pÃ¡gina {PageNumber}", readingState.CurrentPage);
                    return false;
                }

                var message = $"ðŸ“– PÃ¡gina {readingState.CurrentPage + 1} (pÃ¡gina extra solicitada)";
                var sent = await _whatsAppService.SendImageAsync(pageImage, message);

                if (sent)
                {
                    readingState.CurrentPage++;
                    await UpdateReadingStateAsync(readingState);
                    
                    _logger.LogInformation("PÃ¡gina extra {PageNumber} enviada correctamente", readingState.CurrentPage);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en SendNextPageAsync");
                return false;
            }
        }

        private async Task<ReadingState> GetReadingStateAsync()
        {
            return await _readingStateCollection.Find(_ => true).FirstOrDefaultAsync();
        }

        private async Task UpdateReadingStateAsync(ReadingState readingState)
        {
            await _readingStateCollection.ReplaceOneAsync(
                x => x.Id == readingState.Id,
                readingState);
        }
    }
}
