using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NurseriesNetwork.Core.DTOs.AI;

namespace NurseriesNetwork.AI.Services;

public class GroqService : ILlmService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<GroqService> _logger;
    private const string BaseUrl = "https://api.groq.com/openai/v1/chat/completions";

    public GroqService(
        HttpClient httpClient,
        IConfiguration config,
        ILogger<GroqService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
    }

    // ===========================
    // Chat عادي
    // ===========================
    public async Task<string> GetChatResponseAsync(string systemPrompt, string userMessage, List<ConversationMessage>? history = null)
    {
        var requestBody = BuildRequest(systemPrompt, userMessage, history: history);
        var response = await SendAsync(requestBody);

        if (response is null)
            return "معذرة، خدمة الذكاء الاصطناعي غير متاحة حالياً، حاول مرة أخرى بعد قليل.";

        return ExtractTextContent(response.Value) ?? "معذرة، حصلت مشكلة في الرد";
    }

    // ===========================
    // Intent Classification
    // ===========================
    public async Task<IntentClassificationResult> ClassifyIntentAsync(
    string userMessage,
    List<ConversationMessage>? history = null)
    {
        var systemInstruction = """
        أنت Classifier مهمتك تحديد نوع رسالة المستخدم في تطبيق حضانات.
        رد فقط بـ JSON بدون أي شرح أو Markdown، بالشكل ده بالضبط:

        {
          "intent": "GREETING" | "THANKS" | "GENERAL" | "SEARCH" | "BOOKING" | "MEDICAL_CONCERN",
          "reply": "نص الرد المباشر لو الـ intent هو GREETING أو THANKS أو GENERAL أو MEDICAL_CONCERN، غير ذلك سيبه فاضي"
        }

        القواعد:
        - GREETING: لو الرسالة تحية بس (السلام عليكم، أهلاً، ازيك، صباح الخير...)
        - THANKS: لو الرسالة شكر (شكراً، تسلم، ربنا يخليك...)
        - GENERAL: لو سؤال عام عن التطبيق نفسه أو خارج نطاق الحضانات
        - MEDICAL_CONCERN: لو الرسالة بتعبر عن قلق صحي للطفل من غير طلب بحث صريح عن حضانة
        - SEARCH: لو المستخدم بيدور على حضانة أو بيسأل عن حضانات بمعايير معينة
        - BOOKING: لو المستخدم بوضوح عايز يحجز حضانة

        ⚠️ مهم جداً: لو كانت المحادثة السابقة تحدثت عن حجز أو بحث،
        والرسالة الحالية هي استكمال للمحادثة (مثل تاريخ، اسم، تفاصيل إضافية)،
        صنّفها بنفس نوع المحادثة السابقة (BOOKING أو SEARCH) وليس GENERAL أو GREETING.

        لو الـ intent هو GREETING أو THANKS أو GENERAL أو MEDICAL_CONCERN، اكتب رد مناسب وودود بالعربي في حقل reply.
        لو الـ intent هو SEARCH أو BOOKING، سيب حقل reply فاضي "".
        لا تكتب أي حاجة غير الـ JSON.
        """;

        // ✅ مرر history لـ BuildRequest
        var requestBody = BuildRequest(systemInstruction, userMessage, jsonMode: true, history: history);
        var response = await SendAsync(requestBody);

        if (response is null)
            return new IntentClassificationResult("SEARCH", "");

        var text = ExtractTextContent(response.Value);
        if (string.IsNullOrWhiteSpace(text))
            return new IntentClassificationResult("SEARCH", "");

        try
        {
            using var resultDoc = JsonDocument.Parse(text);
            var intent = resultDoc.RootElement.GetProperty("intent").GetString() ?? "SEARCH";
            var reply = resultDoc.RootElement.TryGetProperty("reply", out var replyEl)
                ? replyEl.GetString() ?? "" : "";
            return new IntentClassificationResult(intent, reply);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Groq: Failed to parse intent classification: {Text}", text);
            return new IntentClassificationResult("SEARCH", "");
        }
    }

    // ===========================
    // Extract Search Filters
    // ===========================
    public async Task<SearchFilters> ExtractSearchFiltersAsync(string userMessage)
    {
        var systemInstruction = """
            أنت تستخرج فلاتر بحث من رسالة مستخدم يدور على حضانة.
            رد فقط بـ JSON بدون أي شرح، بالشكل ده بالضبط:

            {
              "city": "اسم المدينة لو مذكورة أو null",
              "maxPrice": رقم السعر الأعلى المطلوب لو مذكور أو null,
              "minRating": رقم التقييم الأدنى المطلوب لو مذكور أو null,
              "childAgeMonths": عمر الطفل بالشهور لو مذكور أو null
            }

            لو القيمة غير مذكورة، استخدم null. لا تخترع قيم. لا تكتب أي حاجة غير الـ JSON.
            """;

        var requestBody = BuildRequest(systemInstruction, userMessage, jsonMode: true);
        var response = await SendAsync(requestBody);

        if (response is null)
            return new SearchFilters(null, null, null, null, ExtractionFailed: true);   // ✅

        var text = ExtractTextContent(response.Value);
        if (string.IsNullOrWhiteSpace(text))
            return new SearchFilters(null, null, null, null);

        try
        {
            using var resultDoc = JsonDocument.Parse(text);
            var root = resultDoc.RootElement;

            string? city = root.TryGetProperty("city", out var c) && c.ValueKind != JsonValueKind.Null
                ? c.GetString() : null;
            decimal? maxPrice = root.TryGetProperty("maxPrice", out var p) && p.ValueKind == JsonValueKind.Number
                ? p.GetDecimal() : null;
            double? minRating = root.TryGetProperty("minRating", out var r) && r.ValueKind == JsonValueKind.Number
                ? r.GetDouble() : null;
            int? childAgeMonths = root.TryGetProperty("childAgeMonths", out var a) && a.ValueKind == JsonValueKind.Number
                ? a.GetInt32() : null;

            return new SearchFilters(city, maxPrice, minRating, childAgeMonths);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Groq: Failed to parse search filters: {Text}", text);
            return new SearchFilters(null, null, null, null, ExtractionFailed: true);   // ✅
        }
    }

    // ===========================
    // Function Calling
    // ===========================
    public async Task<GeminiFunctionCallResult> GetFunctionCallAsync(
    string userMessage,
    List<ConversationMessage>? history = null)
    {
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var tomorrow = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd");
        var nextWeek = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-dd");

        var systemInstruction = $"""
    أنت مساعد ذكي لتطبيق حضانات في مصر اسمه Nurseries Network.

    تاريخ اليوم هو: {today}

    مهمتك هي مساعدة أولياء الأمور في:
    1. البحث عن حضانات مناسبة باستخدام الدالة find_nearby_nurseries.
    2. إنشاء حجز باستخدام الدالة create_booking.

    ========================
    القواعد العامة
    ========================
    - إذا كانت رسالة المستخدم مجرد تحية، فلا تستدعِ أي Function، ورد بتحية ودودة.
    - إذا كانت رسالة المستخدم شكر، فلا تستدعِ أي Function، ورد بلطف.
    - إذا كان السؤال خارج نطاق التطبيق أو الحضانات، فلا تستدعِ أي Function.
    - لا تخترع أي معلومات غير موجودة.
    - لا تخترع أسماء حضانات أو أطفال أو أسعار أو تقييمات أو أماكن.
    - استخدم الـ Functions المتاحة فقط عند الحاجة.
    - لا تستدعِ نفس الـ Function أكثر من مرة لنفس الطلب إلا إذا غيّر المستخدم طلبه.

    ========================
    فهم المحادثة (History)
    ========================

    - استخدم الرسالة الحالية مع History لفهم المقصود.
    - إذا كانت الرسالة الحالية تكمل رسالة سابقة، فاعتبرها استمرارًا لنفس الطلب.
    - إذا أرسل المستخدم اسم الطفل فقط أو التاريخ فقط أو اسم الحضانة فقط، فاجمعها مع المعلومات الموجودة في History.
    - لا تطلب معلومة سبق أن ذكرها المستخدم في المحادثة.
    - ✅ إذا توفرت اسم الحضانة واسم الطفل والتاريخ مجتمعةً عبر المحادثة، استدعِ create_booking مباشرة.

    ========================
    البحث عن الحضانات
    ========================

    استخدم find_nearby_nurseries عندما يطلب المستخدم:

    - حضانة في مدينة معينة.
    - حضانة قريبة.
    - حضانة بسعر معين.
    - حضانة بتقييم معين.
    - حضانة مناسبة لعمر الطفل.

    إذا كانت المعلومات غير كافية، اسأل المستخدم عنها أولًا.

    ========================
    قواعد الحجز
    ========================

    لا تستدعِ create_booking إلا إذا أصبحت المعلومات التالية متوفرة:

    1. اسم الحضانة.
    2. اسم الطفل.
    3. تاريخ بداية الحجز.

    إذا كانت أي معلومة ناقصة، فلا تستدعِ الدالة.

    إذا كان التاريخ فقط هو الناقص، فرد بهذه الرسالة:
    "تمام! لإكمال الحجز، يرجى تزويدي بتاريخ البدء بصيغة YYYY-MM-DD، مثال: 2026-09-01"

    ========================
    فهم التاريخ
    ========================

    إذا قال المستخدم (اليوم / النهارده / تاريخ اليوم / دلوقتي)، فاستخدم: {today}
    إذا قال (بكره / غداً)، فاستخدم: {tomorrow}
    إذا قال (الأسبوع الجاي)، فاستخدم: {nextWeek}

    ⚠️ تحذير مهم: لا تخترع تاريخًا من عندك تحت أي ظرف، حتى لو بدا منطقيًا.
    إذا لم يذكر المستخدم تاريخًا صريحًا أو تعبيرًا زمنيًا من الأمثلة أعلاه،
    فاطلب منه التاريخ ولا تستدعِ create_booking.

    ========================
    بعد تنفيذ الحجز
    ========================

    - لا تؤكد نجاح الحجز إلا إذا أكدت الدالة create_booking نجاح العملية.
    - إذا أعادت الدالة رسالة خطأ، فاشرح سبب الخطأ للمستخدم.
    - إذا نجحت العملية، أخبر المستخدم بنجاح الحجز مع ملخص بسيط.

    ========================
    أسلوب الرد
    ========================

    - استخدم اللغة العربية دائمًا ما لم يطلب المستخدم غير ذلك.
    - اجعل الردود قصيرة وواضحة.
    - كن ودودًا وطبيعيًا.
    - لا تكرر نفس المعلومات.
    - لا تضف معلومات لم يطلبها المستخدم.
    """;



        // ✅ بعد — OpenAI format (الصح مع Groq)
        var tools = new object[]
        {
    new
    {
        type = "function",
        function = new
        {
            name = "find_nearby_nurseries",
            description = "البحث عن حضانات قريبة من موقع معين بسعر معين",
            parameters = new
            {
                type = "object",
                properties = new
                {
                    city = new { type = "string", description = "اسم المدينة المطلوب البحث فيها" },
                    max_price = new { type = "number", description = "أعلى سعر يومي مقبول بالجنيه المصري" }
                },
                required = Array.Empty<string>()
            }
        }
    },
    new
    {
        type = "function",
        function = new
        {
            name = "create_booking",
            description = "إنشاء حجز جديد لطفل في حضانة معينة — استدعِ هذه الدالة فقط عندما يكون لديك nursery_name وchild_name وstart_date بشكل صريح من المستخدم",
            parameters = new
            {
                type = "object",
                properties = new
                {
                    nursery_name = new { type = "string", description = "اسم الحضانة المطلوب الحجز فيها" },
                    child_name = new { type = "string", description = "اسم الطفل المطلوب حجزه" },
                    start_date = new
                    {
                        type = "string",
                        description = "تاريخ بدء الحجز بصيغة YYYY-MM-DD مثال 2026-09-01. مهم: لا تخترع هذه القيمة أبداً، إذا لم يذكرها المستخدم اطلبها منه."
                    }
                },
                required = new[] { "nursery_name", "child_name" }
            }
        }
    }
        };


        var model = _config["Groq:ChatModel"];

        var messages = new List<object>();
        messages.Add(new { role = "system", content = systemInstruction });

        if (history != null)
            foreach (var msg in history)
                messages.Add(new { role = msg.Role, content = msg.Content });

        messages.Add(new { role = "user", content = userMessage });

        var requestBody = new { model, messages, tools };

        var response = await SendAsync(requestBody);

        if (response is null)
            return new GeminiFunctionCallResult(false, null, null, "حصل خطأ في فهم الطلب");

        var root = response.Value;
        var message = root.GetProperty("choices")[0].GetProperty("message");

        if (message.TryGetProperty("tool_calls", out var toolCalls) &&
            toolCalls.ValueKind == JsonValueKind.Array &&
            toolCalls.GetArrayLength() > 0)
        {
            var firstCall = toolCalls[0];
            var functionName = firstCall.GetProperty("function").GetProperty("name").GetString()!;
            var argsJson = firstCall.GetProperty("function").GetProperty("arguments").GetString()!;

            _logger.LogInformation(
                "Groq decided to call function: {FunctionName} with args: {Args}",
                functionName, argsJson);

            return new GeminiFunctionCallResult(true, functionName, argsJson, null);
        }

        var textContent = message.TryGetProperty("content", out var contentEl)
            ? contentEl.GetString() : null;

        return new GeminiFunctionCallResult(
            false, null, null,
            textContent ?? "معذرة، مش فاهم طلبك، ممكن توضحه أكتر؟");
    }

    // ===========================
    // بعد تنفيذ الـ Function، نرجع النتيجة للموديل يصيغها كرد طبيعي
    // ===========================

    // استبدل GetFinalResponseAfterFunctionAsync في GroqService.cs بالكود ده
    // ===========================
    public async Task<string> GetFinalResponseAfterFunctionAsync(
        string userMessage, string functionName, string functionResult)
    {
        var systemInstruction = """
        أنت مساعد ذكي لتطبيق حضانات في مصر.
        مهمتك: صياغة رد طبيعي وودود بالعربية الفصيحة فقط بناءً على نتيجة العملية.
 
        قواعد صارمة:
        - اكتب بالعربية فقط، ولا تستخدم أي كلمات أجنبية أو رموز من لغات أخرى إطلاقاً.
        - اكتب بالعربية فقط، ولا تستخدم أي كلمات أجنبية أو رموز من لغات أخرى إطلاقاً.
        - اكتب بالعربية فقط، ولا تستخدم أي كلمات أجنبية أو رموز من لغات أخرى إطلاقاً.
        - لا تخترع معلومات غير موجودة في النتيجة.
        - كن مختصراً وودوداً.
        - لو النتيجة تحتوي على بيانات JSON، لخصها بشكل بشري مفهوم.
        - لو النتيجة رسالة خطأ أو اعتذار، وضح للمستخدم بأدب ماذا حدث.
        """;

        var prompt = $"""
        طلب المستخدم: {userMessage}
        العملية المنفذة: {functionName}
        النتيجة: {functionResult}
        """;

        var requestBody = BuildRequest(systemInstruction, prompt);
        var response = await SendAsync(requestBody);

        if (response is null)
            return functionResult;

        return ExtractTextContent(response.Value) ?? functionResult;
    }


    // ===========================
    // Helpers
    // ===========================
    private object BuildRequest(
    string? systemPrompt, string userMessage,
    bool jsonMode = false,
    List<ConversationMessage>? history = null)
    {
        var model = _config["Groq:ChatModel"];

        var messages = new List<object>();

        if (!string.IsNullOrWhiteSpace(systemPrompt))
            messages.Add(new { role = "system", content = systemPrompt });

        // ✅ ضيف الـ history قبل الرسالة الحالية
        if (history != null)
        {
            foreach (var msg in history)
            {
                messages.Add(new { role = msg.Role, content = msg.Content });
            }
        }

        messages.Add(new { role = "user", content = userMessage });

        if (jsonMode)
            return new { model, messages, response_format = new { type = "json_object" } };

        return new { model, messages };
    }
    private async Task<JsonElement?> SendAsync(object requestBody)
    {
        var apiKey = _config["Groq:ApiKey"];

        using var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl)
        {
            Content = JsonContent.Create(requestBody)
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError(
                "Groq call failed. StatusCode: {StatusCode}. Response: {Error}",
                response.StatusCode, error);
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    private static string? ExtractTextContent(JsonElement root)
    {
        return root
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();
    }






    // ===========================
    // ✅ جديد — Function Calling خاص بـ NurseryAdmin (نفس منطق Gemini بصيغة OpenAI-compatible)
    // ضيف الميثود دي جوه كلاس GroqService الموجود عندك، في أي مكان بعد GetFunctionCallAsync
    // ===========================
    public async Task<AdminFunctionCallResult> GetAdminFunctionCallAsync(string userMessage, List<ConversationMessage>? history = null)
    {
        var systemInstruction = """
        أنت مساعد ذكي يساعد مدير حضانة (NurseryAdmin) على إدارة حضانته بكفاءة.

        مهمتك:
        1. لو سأل عن أداء حضانته (إيرادات، حجوزات، تقييم) استخدم get_nursery_performance.
        2. لو طلب البحث أو الفلترة في حجوزاته (حسب الحالة، الدفع، عمر الطفل، فترة زمنية) استخدم search_my_bookings.

        قواعد:
        - إذا كانت الرسالة تحية أو شكر أو غير متعلقة بإدارة الحضانة، لا تستخدم أي Function، رد مباشرة بأدب.
        - حول الفترات الزمنية الكلامية لعدد أيام تقريبي (مثلاً "الأسبوع ده" = 7, "الشهر ده" = 30).
        - حول "أقل من سنة" إلى max_child_age_months = 12، و"أكبر من سنتين" إلى min_child_age_months = 24، وهكذا.
        - لا تخترع بيانات أو نتائج، اعتمد فقط على نتيجة تنفيذ الـ Function.
        """;

        var tools = new object[]
        {
        new
        {
            type = "function",
            function = new
            {
                name = "get_nursery_performance",
                description = "تحليل أداء حضانة المستخدم (عدد الحجوزات، المدفوعات، الإيرادات، التقييم) للشهر الحالي",
                parameters = new
                {
                    type = "object",
                    properties = new { },
                    required = Array.Empty<string>()
                }
            }
        },
        new
        {
            type = "function",
            function = new
            {
                name = "search_my_bookings",
                description = "البحث الذكي في حجوزات حضانة المستخدم بفلاتر متعددة",
                parameters = new
                {
                    type = "object",
                    properties = new
                    {
                        booking_status = new { type = "string", description = "حالة الحجز: Pending, Confirmed, Cancelled, Completed" },
                        payment_status = new { type = "string", description = "حالة الدفع: Pending, Paid, Failed, Refunded" },
                        max_child_age_months = new { type = "integer", description = "أقصى عمر للطفل بالشهور" },
                        min_child_age_months = new { type = "integer", description = "أقل عمر للطفل بالشهور" },
                        within_last_days = new { type = "integer", description = "عدد الأيام الماضية للفلترة الزمنية" }
                    },
                    required = Array.Empty<string>()
                }
            }
        }
        };

        var model = _config["Groq:ChatModel"];

        var messages = new List<object>();
        messages.Add(new { role = "system", content = systemInstruction });

        if (history != null)
            foreach (var msg in history)
                messages.Add(new { role = msg.Role, content = msg.Content });

        messages.Add(new { role = "user", content = userMessage });

        var requestBody = new { model, messages, tools };

        var response = await SendAsync(requestBody);

        if (response is null)
            return new AdminFunctionCallResult(false, null, null, "حصل خطأ في فهم الطلب");

        var root = response.Value;
        var message = root.GetProperty("choices")[0].GetProperty("message");

        if (message.TryGetProperty("tool_calls", out var toolCalls) &&
            toolCalls.ValueKind == System.Text.Json.JsonValueKind.Array &&
            toolCalls.GetArrayLength() > 0)
        {
            var firstCall = toolCalls[0];
            var functionName = firstCall.GetProperty("function").GetProperty("name").GetString()!;
            var argsJson = firstCall.GetProperty("function").GetProperty("arguments").GetString()!;

            _logger.LogInformation(
                "Groq decided to call admin function: {FunctionName} with args: {Args}",
                functionName, argsJson);

            return new AdminFunctionCallResult(true, functionName, argsJson, null);
        }

        var textContent = message.TryGetProperty("content", out var contentEl)
            ? contentEl.GetString() : null;

        return new AdminFunctionCallResult(
            false, null, null,
            textContent ?? "معذرة، مش فاهم طلبك، ممكن توضحه أكتر؟");
    }
}