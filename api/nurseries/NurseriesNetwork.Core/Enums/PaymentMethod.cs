using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Core.Enums
{
    public enum PaymentMethod
    {
        VodafoneCash,   // عن طريق Paymob
        Meeza,          // عن طريق Paymob
        Card,           // Visa/Mastercard عن طريق Paymob
        PayPal          // عن طريق PayPal مباشرة
    }
}
