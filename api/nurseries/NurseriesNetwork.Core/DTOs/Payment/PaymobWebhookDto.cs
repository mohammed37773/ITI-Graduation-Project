namespace NurseriesNetwork.Core.DTOs.Payment;

// شكل الداتا اللي Paymob بيبعتها في الـ Webhook (Transaction Callback)
public record PaymobWebhookDto(
    string Hmac,
    PaymobTransactionObj Obj
);

public record PaymobTransactionObj(
    long Id,
    bool Success,
    decimal AmountCents,
    string? Currency,
    PaymobOrderObj Order,
    string? IntegrationId
);

public record PaymobOrderObj(
    long Id,
    long MerchantOrderId
);