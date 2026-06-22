
namespace NurseriesNetwork.Core.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string email, string otpCode);


        //Task SendConfirmationEmailAsync(string email, string token);
        //Task SendBookingConfirmationAsync(string email, int bookingId);
    }
}
