namespace NurseriesNetwork.Core.DTOs.AI;

public class ChatRequestDto
{
    public string Message { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    // ✅ جديد — تاريخ المحادثة السابقة (الـ Frontend بيبعته مع كل request)
    // كل عنصر فيه { Role: "user"/"assistant", Content: "نص الرسالة" }
    public List<ConversationMessage> History { get; set; } = new();
}

public class ConversationMessage
{
    public string Role { get; set; } = string.Empty;    // "user" أو "assistant"
    public string Content { get; set; } = string.Empty;
}