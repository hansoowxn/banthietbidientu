using System.Net;
using System.Net.Mail;

namespace banthietbidientu.Services
{
    // Tạo Interface để dễ quản lý
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string message);
    }

    // Class thực thi
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            // Đọc cấu hình từ appsettings.json
            string mailServer = _configuration["EmailSettings:MailServer"];
            int mailPort = int.Parse(_configuration["EmailSettings:MailPort"]);
            string senderName = _configuration["EmailSettings:SenderName"];
            string senderEmail = _configuration["EmailSettings:SenderEmail"];
            string password = _configuration["EmailSettings:Password"];

            var client = new SmtpClient(mailServer, mailPort)
            {
                Credentials = new NetworkCredential(senderEmail, password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = subject,
                Body = message,
                IsBodyHtml = true // Cho phép gửi HTML (để hóa đơn đẹp)
            };

            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
        }
    }
}