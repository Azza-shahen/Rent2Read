using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Rent2Read.Web.Services
{
    public class EmailSender : IEmailSender//is a built-in interface
    {
        private readonly MailSettings _mailSettings;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmailSender(IOptions<MailSettings> mailSettings, IWebHostEnvironment webHostEnvironment)
        {
            _mailSettings = mailSettings.Value;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            MailMessage message = new()
            {
                From = new MailAddress(_mailSettings.Email!, _mailSettings.DisplayName),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            message.To.Add(_webHostEnvironment.IsDevelopment() ? "UG_31159876@ics.tanta.edu.eg" : email);

            SmtpClient smtpClient = new(_mailSettings.Host)
            //SmtpClient:is a .NET class used to open a "connection" with the mail server (such as Gmail, Outlook or any SMTP server) in order to send an email from it.
            {
                Port = _mailSettings.Port,
                Credentials = new NetworkCredential(_mailSettings.Email, _mailSettings.Password),//Login data
                EnableSsl = true
            };
            await smtpClient.SendMailAsync(message);
            smtpClient.Dispose();
        }
    }



}
