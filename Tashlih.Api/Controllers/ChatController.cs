using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tashlih.Application.DTOs.Chat;
using Tashlih.Application.Interfaces;

namespace Tashlih.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    /// <summary>
    /// بدء محادثة جديدة (للعميل فقط)
    /// </summary>
    [HttpPost("start")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> StartChat([FromForm] StartChatRequest request)
    {
        if (!IsCustomer())
            return Forbid();

        var customerId = GetUserId();
        var result = await _chatService.StartChatAsync(customerId, request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// جلب محادثاتي
    /// </summary>
    [HttpGet("my-threads")]
    public async Task<IActionResult> GetMyThreads()
    {
        var userId = GetUserId();
        var userType = GetUserType();

        ChatThreadsResponse result;

        if (userType == "customer")
            result = await _chatService.GetCustomerThreadsAsync(userId);
        else
            result = await _chatService.GetSupplierThreadsAsync(userId);

        return Ok(result);
    }
    /// <summary>
    /// جلب رسائل محادثة مع Pagination
    /// </summary>
    /// <param name="threadId">رقم المحادثة</param>
    /// <param name="page">رقم الصفحة (افتراضي: 1)</param>
    /// <param name="pageSize">عدد الرسائل في الصفحة (افتراضي: 20، الحد الأقصى: 50)</param>
    [HttpGet("{threadId}/messages")]
    public async Task<IActionResult> GetThreadMessages(long threadId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        // التحقق من صحة الـ Parameters
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 50) pageSize = 50;

        var userId = GetUserId();
        var userType = GetUserType();

        var result = await _chatService.GetThreadMessagesAsync(userId, userType, threadId, page, pageSize);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// إرسال رسالة (نص و/أو صور و/أو فيديوهات و/أو صوت)
    /// </summary>
    /// <remarks>
    /// يمكن إرسال:
    /// - نص فقط
    /// - صور فقط (مع أو بدون نص)
    /// - فيديوهات فقط (مع أو بدون نص)
    /// - صوت فقط
    /// - نص + صور
    /// - نص + فيديوهات
    /// 
    /// ملاحظة: الصوت يُرسل لوحده فقط (لا يمكن دمجه مع صور أو فيديوهات)
    /// </remarks>
    [HttpPost("{threadId}/send")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> SendMessage(long threadId, [FromForm] SendMessageRequest request)
    {
        // التحقق من وجود محتوى
        var hasContent = !string.IsNullOrWhiteSpace(request.Content);
        var hasImages = request.Images != null && request.Images.Count > 0;
        var hasVideos = request.Videos != null && request.Videos.Count > 0;
        var hasVoice = request.Voice != null;

        if (!hasContent && !hasImages && !hasVideos && !hasVoice)
        {
            return BadRequest(new SendMessageResponse
            {
                Success = false,
                Message = "Message content is required",
                MessageAr = "يجب إرسال محتوى (نص أو صور أو فيديو أو صوت)"
            });
        }

        // الصوت لازم يكون لوحده
        if (hasVoice && (hasImages || hasVideos))
        {
            return BadRequest(new SendMessageResponse
            {
                Success = false,
                Message = "Voice message cannot be combined with images or videos",
                MessageAr = "الرسالة الصوتية لا يمكن دمجها مع صور أو فيديوهات"
            });
        }

        var userId = GetUserId();
        var userType = GetUserType();

        var result = await _chatService.SendMessageAsync(userId, userType, threadId, request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// تعليم المحادثة كمقروءة
    /// </summary>
    [HttpPut("{threadId}/read")]
    public async Task<IActionResult> MarkAsRead(long threadId)
    {
        var userId = GetUserId();
        var userType = GetUserType();

        var result = await _chatService.MarkAsReadAsync(userId, userType, threadId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    /// <summary>
    /// إغلاق المحادثة
    /// </summary>
    [HttpDelete("{threadId}")]
    public async Task<IActionResult> CloseThread(long threadId)
    {
        var userId = GetUserId();
        var userType = GetUserType();

        var result = await _chatService.CloseThreadAsync(userId, userType, threadId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    #region Helper Methods
       
    private long GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(userIdClaim, out var id) ? id : 0;
    }

    private string GetUserType()
    {
        return User.FindFirst("user_type")?.Value ?? "customer";
    }

    private bool IsCustomer()
    {
        return GetUserType() == "customer";
    }

    private bool IsSupplier()
    {
        return GetUserType() == "supplier";
    }

    #endregion
}
