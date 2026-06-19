using NurseriesNetwork.Core.Enums;
using NurseriesNetwork.Core.Interfaces.Services;

namespace NurseriesNetwork.Infrastructure.Services.Payment;

public class PaymentFactory : IPaymentFactory
{
    private readonly PaymobService _paymob;
    private readonly PayPalService _payPal;

    public PaymentFactory(
        PaymobService paymob,
        PayPalService payPal)
    {
        _paymob = paymob;
        _payPal = payPal;
    }

    public IPaymentService GetPaymentService(PaymentMethod method)
    {
        return method switch
        {
            PaymentMethod.VodafoneCash => _paymob,
            PaymentMethod.Meeza => _paymob,
            PaymentMethod.Card => _paymob,
            PaymentMethod.PayPal => _payPal,
            _ => throw new ArgumentException("طريقة الدفع غير معروفة")
        };
    }
}