using Microsoft.AspNetCore.Http;
using Tashlih.Application.DTOs.Chat;

namespace Tashlih.Application.Interfaces;

public interface IChatService
{
    /// <summary>
    /// بدء محادثة جديدة (للعميل)
    /// </summary>
    Task<StartChatResponse> StartChatAsync(long customerId, StartChatRequest request);

    /// <summary>
    /// جلب محادثات العميل
    /// </summary>
    Task<ChatThreadsResponse> GetCustomerThreadsAsync(long customerId);

    /// <summary>
    /// جلب محادثات المورد
    /// </summary>
    Task<ChatThreadsResponse> GetSupplierThreadsAsync(long supplierId);


    /// <summary>
    /// جلب رسائل محادثة مع Pagination
    /// </summary>
    Task<ChatMessagesPagedResponse> GetThreadMessagesAsync(long userId, string userType, long threadId, int page = 1, int pageSize = 20);

    /// <summary>
    /// إرسال رسالة (نص و/أو صور و/أو فيديوهات و/أو صوت)
    /// </summary>
    Task<SendMessageResponse> SendMessageAsync(long userId, string userType, long threadId, SendMessageRequest request);

    /// <summary>
    /// تعليم المحادثة كمقروءة
    /// </summary>
    Task<ChatResponse> MarkAsReadAsync(long userId, string userType, long threadId);

    /// <summary>
    /// إغلاق/حذف المحادثة (Soft Delete)
    /// </summary>
    Task<ChatResponse> CloseThreadAsync(long userId, string userType, long threadId);
}