using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Core.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendConfirmationEmailAsync(string email, string token);
        Task SendBookingConfirmationAsync(string email, int bookingId);
    }
}
