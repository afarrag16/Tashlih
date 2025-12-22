namespace Tashlih.Application.DTOs.Chat;

/// <summary>
/// معلومات الـ Pagination
/// </summary>
public class PaginationInfo
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
}

/// <summary>
/// استجابة رسائل المحادثة مع Pagination
/// </summary>
public class ChatMessagesPagedResponse : ChatResponse
{
    public ChatThreadDto? Thread { get; set; }
    public List<ChatMessageDto>? Messages { get; set; }
    public PaginationInfo? Pagination { get; set; }
}
