using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using NurseriesNetwork.Core.Interfaces.Services;
using System.Net.Mail;

namespace NurseriesNetwork.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendConfirmationEmailAsync(string email, string token)
    {
        var encodedToken = Uri.EscapeDataString(token);
        var confirmLink =
            $"http://localhost:5104/api/auth/confirm-email" +
            $"?email={email}&token={encodedToken}";

        var body = $"""
            <h2>أهلاً بيك في Nurseries Network</h2>
            <p>اضغط على الرابط ده عشان تأكد حسابك:</p>
            <a href="{confirmLink}">تأكيد الحساب</a>
            """;

        await SendEmailAsync(email, "تأكيد حساب Nurseries Network", body);
    }

    public async Task SendBookingConfirmationAsync(string email, int bookingId)
    {
        var body = $"""
            <h2>تم استلام حجزك بنجاح</h2>
            <p>رقم الحجز: <b>{bookingId}</b></p>
            """;

        await SendEmailAsync(email, "تأكيد الحجز", body);
    }

    public async Task SendPaymentConfirmationAsync(string email, int paymentId)
    {
        var body = $"""
            <h2>تم الدفع بنجاح</h2>
            <p>رقم الدفعة: <b>{paymentId}</b></p>
            """;

        await SendEmailAsync(email, "تأكيد الدفع", body);
    }

    private async Task SendEmailAsync(
        string toEmail, string subject, string body)
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(
            _config["Email:DisplayName"],
            _config["Email:UserName"]));

        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = body };

        using var client = new MailKit.Net.Smtp.SmtpClient();

        await client.ConnectAsync(
            _config["Email:Host"],
            int.Parse(_config["Email:Port"]!),
            SecureSocketOptions.StartTls);

        await client.AuthenticateAsync(
            _config["Email:UserName"],
            _config["Email:Password"]);

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}