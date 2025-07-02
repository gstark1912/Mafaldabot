using System.Net;
using System.Net.Mail;

namespace PdfReaderService.WindowsService.Services
{
    public class EmailSenderService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPass;
        private readonly string _fromEmail;

        public EmailSenderService(IConfiguration configuration)
        {
            _smtpServer = configuration["EmailSettings:SmtpServer"];
            _smtpPort = int.Parse(configuration["EmailSettings:SmtpPort"]);
            _smtpUser = configuration["EmailSettings:SmtpUser"];
            _smtpPass = configuration["EmailSettings:SmtpPass"];
            _fromEmail = configuration["EmailSettings:FromEmail"];
        }

        public async Task SendImageAsync(byte[] imageBytes, string imageName, string toEmail, string subject, string body)
        {
            using var message = new MailMessage(_fromEmail, toEmail, subject, body);
            using var stream = new MemoryStream(imageBytes);
            var attachment = new Attachment(stream, imageName, "image/png"); // cambiá el MIME si es otro formato
            message.Attachments.Add(attachment);

            using var client = new SmtpClient(_smtpServer, _smtpPort)
            {
                Credentials = new NetworkCredential(_smtpUser, _smtpPass),
                EnableSsl = true
            };

            await client.SendMailAsync(message);
        }
    }
}