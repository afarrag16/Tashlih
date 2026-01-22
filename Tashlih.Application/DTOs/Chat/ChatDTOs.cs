using Microsoft.AspNetCore.Http;

namespace Tashlih.Application.DTOs.Chat;

#region Request DTOs

/// <summary>
/// طلب بدء محادثة جديدة
/// </summary>
public class StartChatRequest
{
    public long SupplierId { get; set; }
    public long? PartId { get; set; }

    // المحتوى
    public string? Content { get; set; }
    public List<IFormFile>? Images { get; set; }
    public List<IFormFile>? Videos { get; set; }
    public IFormFile? Voice { get; set; }

}

/// <summary>
/// طلب إرسال رسالة (موحد لكل الأنواع)
/// </summary>
public class SendMessageRequest
{
    /// <summary>
    /// النص (اختياري)
    /// </summary>
    public long? PartId { get; set; }
    public string? Content { get; set; }

    /// <summary>
    /// الصور (اختياري) - max 5MB لكل صورة
    /// </summary>
    public List<IFormFile>? Images { get; set; }

    /// <summary>
    /// الفيديوهات (اختياري) - max 50MB لكل فيديو
    /// </summary>
    public List<IFormFile>? Videos { get; set; }

    /// <summary>
    /// التسجيل الصوتي (اختياري) - max 10MB
    /// </summary>
    public IFormFile? Voice { get; set; }
}

#endregion

#region Response DTOs

/// <summary>
/// استجابة عامة للشات
/// </summary>
public class ChatResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? MessageAr { get; set; }
}

/// <summary>
/// استجابة بدء محادثة
/// </summary>
public class StartChatResponse : ChatResponse
{
    public ChatThreadDto? Thread { get; set; }
}

/// <summary>
/// استجابة قائمة المحادثات
/// </summary>
public class ChatThreadsResponse : ChatResponse
{
    public List<ChatThreadListDto>? Threads { get; set; }
}

/// <summary>
/// استجابة رسائل المحادثة
/// </summary>
public class ChatMessagesResponse : ChatResponse
{
    public ChatThreadDto? Thread { get; set; }
    public List<ChatMessageDto>? Messages { get; set; }
}

/// <summary>
/// استجابة إرسال رسالة
/// </summary>
public class SendMessageResponse : ChatResponse
{
    public ChatMessageDto? ChatMessage { get; set; }
}

#endregion

#region Data DTOs

/// <summary>
/// بيانات المحادثة الكاملة
/// </summary>
public class ChatThreadDto
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public long SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public string? SupplierLogoUrl { get; set; }
    public string Status { get; set; } = null!;
    public DateTime? CreatedAt { get; set; }
    public ChatPartDto? Part { get; set; }
}

/// <summary>
/// بيانات المحادثة للقائمة
/// </summary>
public class ChatThreadListDto
{
    public long Id { get; set; }

    // الطرف الآخر
    public long OtherUserId { get; set; }
    public string? OtherUserName { get; set; }
    public string? OtherUserImage { get; set; }

    // آخر رسالة
    public string? LastMessage { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int UnreadCount { get; set; }

    // القطعة (لو موجودة)
    public ChatPartDto? Part { get; set; }

    public string Status { get; set; } = null!;
}

/// <summary>
/// بيانات القطعة المختصرة للشات
/// </summary>
public class ChatPartDto
{
    public long Id { get; set; }
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public string? Status { get; set; }
}

/// <summary>
/// بيانات الرسالة
/// </summary>
public class ChatMessageDto
{
    public long Id { get; set; }
    public long SenderId { get; set; }
    public string SenderType { get; set; } = null!; // customer, supplier
    public string MessageType { get; set; } = null!; // text, image, video, voice, mixed
    public string? Content { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime? CreatedAt { get; set; }
    public List<ChatAttachmentDto>? Attachments { get; set; }
    public ChatPartDto? Part { get; set; }
}

/// <summary>
/// بيانات المرفق
/// </summary>
public class ChatAttachmentDto
{
    public long Id { get; set; }
    public string FileType { get; set; } = null!; // image, video, voice
    public string FileUrl { get; set; } = null!;
    public string? FileName { get; set; }
    public int? FileSize { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? Duration { get; set; } // بالثواني للصوت والفيديو
}

#endregion