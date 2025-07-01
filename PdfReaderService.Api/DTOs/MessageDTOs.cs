namespace PdfReaderService.Api.DTOs
{
    public class SendPageRequest
    {
        public string FilePath { get; set; }
        public int PageNumber { get; set; }
        public byte[] PageImage { get; set; }
        public string Message { get; set; }
    }

    public class WhatsAppMessage
    {
        public byte[] Image { get; set; }
        public string Caption { get; set; }
        public string RecipientNumber { get; set; }
    }
}
