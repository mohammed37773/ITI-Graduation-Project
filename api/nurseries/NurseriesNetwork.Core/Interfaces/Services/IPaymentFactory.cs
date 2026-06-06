using NurseriesNetwork.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Core.Interfaces.Services
{
    public interface IPaymentFactory
    {
        IPaymentService GetPaymentService(PaymentMethod method);
    }
}
