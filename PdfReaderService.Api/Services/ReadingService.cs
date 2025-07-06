using MongoDB.Driver;
using PdfReaderService.Api.Models;

namespace PdfReaderService.Api.Services
{
    public class ReadingService : IReadingService
    {
        private readonly IMongoCollection<ReadingState> _readingStateCollection;
        private readonly IMongoCollection<Fact> _factsCollection;
        private readonly IPdfService _pdfService;
        private readonly IWhatsAppService _whatsAppService;
        private readonly ILogger<ReadingService> _logger;
        private readonly IImageUploadService _imageUploadService;

        public ReadingService(
            IMongoDatabase database,
            IPdfService pdfService,
            IWhatsAppService whatsAppService,
            IImageUploadService imageUploadService,
            ILogger<ReadingService> logger)
        {
            _readingStateCollection = database.GetCollection<ReadingState>("ReadingState");
            _factsCollection = database.GetCollection<Fact>("Facts");
            _pdfService = pdfService;
            _whatsAppService = whatsAppService;
            _imageUploadService = imageUploadService;
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

                var pageImage = _pdfService.GetPageImageAsync(readingState.FilePath, (readingState.CurrentPage + readingState.StartPage));
                if (pageImage == null)
                {
                    _logger.LogError("No se pudo obtener la imagen de la pÃ¡gina {PageNumber}", readingState.CurrentPage);
                    return false;
                }

                var url = await _imageUploadService.UploadImageAsync(pageImage);
                if (url == null)
                {
                    _logger.LogError("No se pudo subir la imagen de la pÃ¡gina {PageNumber}", readingState.CurrentPage);
                    return false;
                }

                var message = await GetMessage(url);
                var sent = await _whatsAppService.SendTextAsync(message);

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

        private async Task<string> GetMessage(string url)
        {
            var totalFacts = await _factsCollection.CountDocumentsAsync(_ => true);

            var dayOfYear = DateTime.UtcNow.DayOfYear;
            var factIndex = dayOfYear % 600;

            var fact = await _factsCollection
                .Find(FilterDefinition<Fact>.Empty)
                .Skip(factIndex)
                .Limit(1)
                .FirstOrDefaultAsync();

            return $"Buen día linda! \n" +
            $"Acá tenés tu página del día: {url} \n\n" +
            "Sobre Mafalda:\n\n" +
            $"{fact.Category} → {fact.Text}\n\n";
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
                    await _whatsAppService.SendTextAsync("¡Has completado toda la lectura!");
                    return true;
                }

                var pageImage = _pdfService.GetPageImageAsync(readingState.FilePath, readingState.CurrentPage);
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
