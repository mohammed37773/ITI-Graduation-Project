using MimeKit;
using Microsoft.Extensions.Configuration;
using NurseriesNetwork.Core.Interfaces.Services;

namespace NurseriesNetwork.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendOtpEmailAsync(string email, string otpCode)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Nurseries Network", _configuration["EmailSettings:FromAddress"]));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = "Your OTP Verification Code";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $"<h3>Welcome to Nurseries Network</h3><p>Your OTP code is: <b>{otpCode}</b>. It is valid for 5 minutes.</p>"
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new MailKit.Net.Smtp.SmtpClient();

            await client.ConnectAsync(
                _configuration["EmailSettings:Server"],
                int.Parse(_configuration["EmailSettings:Port"] ?? "587"),
                MailKit.Security.SecureSocketOptions.StartTls 
            );

            await client.AuthenticateAsync(
                _configuration["EmailSettings:Username"],
                _configuration["EmailSettings:Password"]
            );

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}