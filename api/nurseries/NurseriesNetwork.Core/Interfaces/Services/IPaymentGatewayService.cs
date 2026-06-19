namespace NurseriesNetwork.Core.Interfaces.Services;

public interface IPaymentGatewayService
{
    // بيتأكد إن الـ Webhook جاي فعلاً من Paymob أو PayPal
    bool VerifySignature(string payload, string receivedSignature);
}