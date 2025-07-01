namespace PdfReaderService.Api.Services
{
    public interface IReadingService
    {
        Task<bool> SendDailyPageAsync();
        Task<bool> SendNextPageAsync();
    }

    public interface IPdfService
    {
        Task<byte[]> GetPageImageAsync(string filePath, int pageNumber);
        Task<int> GetTotalPagesAsync(string filePath);
    }

    public interface IWhatsAppService
    {
        Task<bool> SendImageAsync(byte[] imageData, string caption);
        Task<bool> SendTextAsync(string message);
    }
}
