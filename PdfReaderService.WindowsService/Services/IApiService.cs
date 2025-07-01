namespace PdfReaderService.WindowsService.Services
{
    public interface IApiService
    {
        Task<bool> SendDailyPageAsync();
        Task<bool> SendNextPageAsync();
    }
}
