namespace NurseriesNetwork.Core.DTOs.AI;

public record IntentClassificationResult(
    string Intent,
    string DirectReply
);

// ✅ ExtractionFailed = true يعني إن استخراج الفلاتر فشل بسبب خطأ API (لا يجب اعتباره "مفيش فلاتر")
//    بينما لو كل القيم null و ExtractionFailed = false، يعني المستخدم فعليًا مفيش في رسالته فلاتر صريحة
public record SearchFilters(
    string? City,
    decimal? MaxPrice,
    double? MinRating,
    int? ChildAgeMonths,
    bool ExtractionFailed = false
);

public record GeminiFunctionCallResult(
    bool ShouldCallFunction,
    string? FunctionName,
    string? ArgumentsJson,
    string? DirectTextResponse
);

public record RecommendationResult(
    string ResponseText,
    List<NurseryDto> Nurseries
);

public record NurseryDto(
    int Id,
    string Name,
    decimal DailyPrice,
    double AvgRating,
    string? City,
    string? Address
);

// ✅ النتيجة الخام لتحليل أداء الحضانة، قبل ما يصيغها الـ LLM كنص بشري
public record NurseryPerformanceData(
    string NurseryName,
    int TotalBookingsThisMonth,
    int PaidBookingsThisMonth,
    int PendingBookingsThisMonth,
    decimal TotalRevenueThisMonth,
    double CurrentAvgRating,
    int TotalReviewsCount
);

// ✅ فلاتر البحث الذكي في حجوزات الحضانة بتاعة الـ Admin
public record AdminBookingSearchFilters(
    string? BookingStatus,      // Pending, Confirmed, Cancelled, Completed
    string? PaymentStatus,      // Pending, Paid, Failed, Refunded
    int? MaxChildAgeMonths,     // فلتر "أطفال عمرهم أقل من سنة"
    int? MinChildAgeMonths,
    int? WithinLastDays         // فلتر "الأسبوع ده" مثلاً = 7
);

// ✅ نتيجة Function Calling الخاصة بالـ Admin Agent (موازية لـ GeminiFunctionCallResult بتاع Parent)
public record AdminFunctionCallResult(
    bool ShouldCallFunction,
    string? FunctionName,
    string? ArgumentsJson,
    string? DirectTextResponse
);