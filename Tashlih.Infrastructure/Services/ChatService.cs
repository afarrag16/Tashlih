using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Tashlih.Application.DTOs.Chat;
using Tashlih.Application.Interfaces;
using Tashlih.Core.Entities;
using Tashlih.Infrastructure.Models;

namespace Tashlih.Infrastructure.Services;

public class ChatService : IChatService
{
    private readonly TashlihContext _context;
    private readonly IFileService _fileService;
    private readonly IChatHubService _chatHubService;

    // حدود الملفات
    private const int MaxImageSize = 5 * 1024 * 1024;      // 5MB
    private const int MaxVideoSize = 50 * 1024 * 1024;     // 50MB
    private const int MaxVoiceSize = 10 * 1024 * 1024;     // 10MB

    public ChatService(TashlihContext context, IFileService fileService, IChatHubService chatHubService)
    {
        _context = context;
        _fileService = fileService;
        _chatHubService = chatHubService;
    }

    /// <summary>
    /// بدء محادثة جديدة (للعميل)
    /// </summary>
    public async Task<StartChatResponse> StartChatAsync(long customerId, StartChatRequest request)
    {
        // التحقق من وجود المورد
        var supplier = await _context.SupplierProfiles
            .FirstOrDefaultAsync(s => s.Id == request.SupplierId && s.IsVerified && s.Status == "active" && s.DeletedAt == null);

        if (supplier == null)
        {
            return new StartChatResponse
            {
                Success = false,
                Message = "Supplier not found",
                MessageAr = "المورد غير موجود"
            };
        }

        // التحقق من وجود القطعة (لو موجودة)
        Part? part = null;
        if (request.PartId.HasValue)
        {
            part = await _context.Parts
                .Include(p => p.PartImages.Where(i => i.IsPrimary))
                .FirstOrDefaultAsync(p => p.Id == request.PartId && p.SupplierId == request.SupplierId && p.DeletedAt == null);

            if (part == null)
            {
                return new StartChatResponse
                {
                    Success = false,
                    Message = "Part not found",
                    MessageAr = "القطعة غير موجودة"
                };
            }
        }

        // التحقق من وجود محادثة سابقة مع نفس المورد والقطعة
        var existingThread = await _context.ChatThreads
            .FirstOrDefaultAsync(t => t.CustomerId == customerId
                && t.SupplierId == request.SupplierId
                && t.PartId == request.PartId
                && t.Status == "active");

        if (existingThread != null)
        {
            // إرسال الرسالة في المحادثة الموجودة لو في محتوى
            if (!string.IsNullOrWhiteSpace(request.Content) ||
                (request.Images != null && request.Images.Count > 0) ||
                (request.Videos != null && request.Videos.Count > 0) ||
                request.Voice != null)
            {
                var existingSendRequest = new SendMessageRequest
                {
                    Content = request.Content,
                    Images = request.Images,
                    Videos = request.Videos,
                    Voice = request.Voice
                };

                await SendMessageAsync(customerId, "customer", existingThread.Id, existingSendRequest);
            }

            return new StartChatResponse
            {
                Success = true,
                Message = "Chat already exists",
                MessageAr = "المحادثة موجودة مسبقاً",
                Thread = await MapToThreadDto(existingThread, part, supplier, customerId)
            };
        }

        // التحقق من وجود محتوى - لازم يكون فيه رسالة عشان تبدأ المحادثة
        var hasContent = !string.IsNullOrWhiteSpace(request.Content);
        var hasImages = request.Images != null && request.Images.Count > 0;
        var hasVideos = request.Videos != null && request.Videos.Count > 0;
        var hasVoice = request.Voice != null;

        if (!hasContent && !hasImages && !hasVideos && !hasVoice)
        {
            return new StartChatResponse
            {
                Success = false,
                Message = "Message is required to start a chat",
                MessageAr = "يجب إرسال رسالة لبدء المحادثة"
            };
        }

        // إنشاء محادثة جديدة
        var customer = await _context.Users.FindAsync(customerId);

        var thread = new ChatThread
        {
            CustomerId = customerId,
            SupplierId = request.SupplierId,
            PartId = request.PartId,
            Status = "active",
            CustomerUnreadCount = 0,
            SupplierUnreadCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.ChatThreads.Add(thread);
        await _context.SaveChangesAsync();

        // إرسال الرسالة الأولى
        var newSendRequest = new SendMessageRequest
        {
            Content = request.Content,
            Images = request.Images,
            Videos = request.Videos,
            Voice = request.Voice
        };

        await SendMessageAsync(customerId, "customer", thread.Id, newSendRequest);

        return new StartChatResponse
        {
            Success = true,
            Message = "Chat started successfully",
            MessageAr = "تم بدء المحادثة بنجاح",
            Thread = await MapToThreadDto(thread, part, supplier, customerId)
        };
    }

    /// <summary>
    /// جلب محادثات العميل
    /// </summary>
    public async Task<ChatThreadsResponse> GetCustomerThreadsAsync(long customerId)
    {
        var threads = await _context.ChatThreads
            .Include(t => t.Supplier)
            .Include(t => t.Part)
                .ThenInclude(p => p!.PartImages.Where(i => i.IsPrimary))
            .Where(t => t.CustomerId == customerId && t.Status == "active")
            .OrderByDescending(t => t.LastMessageAt ?? t.CreatedAt)
            .ToListAsync();

        var threadDtos = threads.Select(t => new ChatThreadListDto
        {
            Id = t.Id,
            OtherUserId = t.SupplierId,
            OtherUserName = t.Supplier.BusinessNameAr,
            OtherUserImage = t.Supplier.LogoUrl,
            LastMessage = t.LastMessage,
            LastMessageAt = t.LastMessageAt,
            UnreadCount = t.CustomerUnreadCount,
            Part = t.Part != null ? new ChatPartDto
            {
                Id = t.Part.Id,
                NameAr = t.Part.NameAr,
                NameEn = t.Part.NameEn,
                Price = t.Part.Price,
                ImageUrl = t.Part.PartImages.FirstOrDefault()?.ImageUrl,
                Status = t.Part.Status
            } : null,
            Status = t.Status
        }).ToList();

        return new ChatThreadsResponse
        {
            Success = true,
            Threads = threadDtos
        };
    }

    /// <summary>
    /// جلب محادثات المورد
    /// </summary>
    public async Task<ChatThreadsResponse> GetSupplierThreadsAsync(long supplierId)
    {
        var threads = await _context.ChatThreads
            .Include(t => t.Customer)
            .Include(t => t.Part)
                .ThenInclude(p => p!.PartImages.Where(i => i.IsPrimary))
            .Where(t => t.SupplierId == supplierId && t.Status == "active")
            .OrderByDescending(t => t.LastMessageAt ?? t.CreatedAt)
            .ToListAsync();

        var threadDtos = threads.Select(t => new ChatThreadListDto
        {
            Id = t.Id,
            OtherUserId = t.CustomerId,
            OtherUserName = t.Customer.FullName,
            OtherUserImage = t.Customer.AvatarUrl,
            LastMessage = t.LastMessage,
            LastMessageAt = t.LastMessageAt,
            UnreadCount = t.SupplierUnreadCount,
            Part = t.Part != null ? new ChatPartDto
            {
                Id = t.Part.Id,
                NameAr = t.Part.NameAr,
                NameEn = t.Part.NameEn,
                Price = t.Part.Price,
                ImageUrl = t.Part.PartImages.FirstOrDefault()?.ImageUrl,
                Status = t.Part.Status
            } : null,
            Status = t.Status
        }).ToList();

        return new ChatThreadsResponse
        {
            Success = true,
            Threads = threadDtos
        };
    }

    /// <summary>
    /// جلب رسائل محادثة مع Pagination
    /// </summary>
    public async Task<ChatMessagesPagedResponse> GetThreadMessagesAsync(long userId, string userType, long threadId, int page = 1, int pageSize = 20)
    {
        var thread = await _context.ChatThreads
            .Include(t => t.Customer)
            .Include(t => t.Supplier)
            .Include(t => t.Part)
                .ThenInclude(p => p!.PartImages.Where(i => i.IsPrimary))
            .FirstOrDefaultAsync(t => t.Id == threadId);

        if (thread == null)
        {
            return new ChatMessagesPagedResponse
            {
                Success = false,
                Message = "Thread not found",
                MessageAr = "المحادثة غير موجودة"
            };
        }

        // التحقق من صلاحية الوصول
        if ((userType == "customer" && thread.CustomerId != userId) ||
            (userType == "supplier" && thread.SupplierId != userId))
        {
            return new ChatMessagesPagedResponse
            {
                Success = false,
                Message = "Access denied",
                MessageAr = "غير مصرح لك بالوصول لهذه المحادثة"
            };
        }

        // حساب الـ Pagination
        var totalCount = await _context.ChatMessages
            .CountAsync(m => m.ThreadId == threadId && !m.IsDeleted);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        // جلب الرسائل مع Pagination (من الأقدم للأحدث)
        var messages = await _context.ChatMessages
            .Include(m => m.ChatAttachments)
            .Where(m => m.ThreadId == threadId && !m.IsDeleted)
            .OrderByDescending(m => m.CreatedAt)  // الأحدث أولاً
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .OrderBy(m => m.CreatedAt)  // نرجعهم بالترتيب الصحيح
            .ToListAsync();

        var messageDtos = messages.Select(m => new ChatMessageDto
        {
            Id = m.Id,
            SenderId = m.SenderId,
            SenderType = m.SenderType,
            MessageType = m.MessageType,
            Content = m.Content,
            IsRead = m.IsRead,
            ReadAt = m.ReadAt,
            CreatedAt = m.CreatedAt,
            Attachments = m.ChatAttachments.Select(a => new ChatAttachmentDto
            {
                Id = a.Id,
                FileType = a.FileType,
                FileUrl = a.FileUrl,
                FileName = a.FileName,
                FileSize = a.FileSize,
                ThumbnailUrl = a.ThumbnailUrl,
                Width = a.Width,
                Height = a.Height,
                Duration = a.Duration
            }).ToList()
        }).ToList();

        var threadDto = new ChatThreadDto
        {
            Id = thread.Id,
            CustomerId = thread.CustomerId,
            CustomerName = thread.Customer.FullName,
            SupplierId = thread.SupplierId,
            SupplierName = thread.Supplier.BusinessNameAr,
            SupplierLogoUrl = thread.Supplier.LogoUrl,
            Status = thread.Status,
            CreatedAt = thread.CreatedAt,
            Part = thread.Part != null ? new ChatPartDto
            {
                Id = thread.Part.Id,
                NameAr = thread.Part.NameAr,
                NameEn = thread.Part.NameEn,
                Price = thread.Part.Price,
                ImageUrl = thread.Part.PartImages.FirstOrDefault()?.ImageUrl,
                Status = thread.Part.Status
            } : null
        };

        return new ChatMessagesPagedResponse
        {
            Success = true,
            Thread = threadDto,
            Messages = messageDtos,
            Pagination = new PaginationInfo
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalCount = totalCount
            }
        };
    }   

    /// <summary>
    /// إرسال رسالة (نص و/أو صور و/أو فيديوهات و/أو صوت)
    /// </summary>
    public async Task<SendMessageResponse> SendMessageAsync(long userId, string userType, long threadId, SendMessageRequest request)
    {
        var thread = await _context.ChatThreads.FindAsync(threadId);

        if (thread == null)
        {
            return new SendMessageResponse
            {
                Success = false,
                Message = "Thread not found",
                MessageAr = "المحادثة غير موجودة"
            };
        }

        // التحقق من صلاحية الوصول
        if ((userType == "customer" && thread.CustomerId != userId) ||
            (userType == "supplier" && thread.SupplierId != userId))
        {
            return new SendMessageResponse
            {
                Success = false,
                Message = "Access denied",
                MessageAr = "غير مصرح لك بالوصول لهذه المحادثة"
            };
        }

        // تحديد المحتوى الموجود
        var hasContent = !string.IsNullOrWhiteSpace(request.Content);
        var hasImages = request.Images != null && request.Images.Count > 0;
        var hasVideos = request.Videos != null && request.Videos.Count > 0;
        var hasVoice = request.Voice != null;

        // التحقق من وجود محتوى
        if (!hasContent && !hasImages && !hasVideos && !hasVoice)
        {
            return new SendMessageResponse
            {
                Success = false,
                Message = "Message content is required",
                MessageAr = "يجب إرسال محتوى (نص أو صور أو فيديو أو صوت)"
            };
        }

        // الصوت لازم يكون لوحده
        if (hasVoice && (hasImages || hasVideos))
        {
            return new SendMessageResponse
            {
                Success = false,
                Message = "Voice message cannot be combined with images or videos",
                MessageAr = "الرسالة الصوتية لا يمكن دمجها مع صور أو فيديوهات"
            };
        }

        // التحقق من حجم الملفات
        var validationResult = ValidateFiles(request);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        // تحديد نوع الرسالة
        var messageType = DetermineMessageType(hasContent, hasImages, hasVideos, hasVoice);

        // إنشاء الرسالة
        var message = new ChatMessage
        {
            ThreadId = threadId,
            SenderId = userId,
            SenderType = userType,
            MessageType = messageType,
            Content = request.Content,
            IsRead = false,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();

        // رفع الملفات وإنشاء المرفقات
        var attachments = new List<ChatAttachment>();
        var folder = $"chat/{threadId}";

        // رفع الصور
        if (hasImages)
        {
            foreach (var file in request.Images!)
            {
                var fileUrl = await _fileService.UploadFileAsync(file, $"{folder}/images");
                attachments.Add(CreateAttachment(message.Id, "image", fileUrl, file));
            }
        }

        // رفع الفيديوهات
        if (hasVideos)
        {
            foreach (var file in request.Videos!)
            {
                var fileUrl = await _fileService.UploadFileAsync(file, $"{folder}/videos");
                attachments.Add(CreateAttachment(message.Id, "video", fileUrl, file));
            }
        }

        // رفع الصوت
        if (hasVoice)
        {
            var fileUrl = await _fileService.UploadFileAsync(request.Voice!, $"{folder}/voice");
            attachments.Add(CreateAttachment(message.Id, "voice", fileUrl, request.Voice!));
        }

        // حفظ المرفقات
        if (attachments.Any())
        {
            _context.ChatAttachments.AddRange(attachments);
        }

        // تحديث المحادثة
        thread.LastMessage = GetLastMessageText(request, hasContent, hasImages, hasVideos, hasVoice);
        thread.LastMessageAt = DateTime.UtcNow;
        thread.LastMessageBy = userId;
        thread.UpdatedAt = DateTime.UtcNow;

        // زيادة عداد الرسائل غير المقروءة للطرف الآخر
        if (userType == "customer")
            thread.SupplierUnreadCount++;
        else
            thread.CustomerUnreadCount++;

        await _context.SaveChangesAsync();

        // تجهيز DTO للرسالة
        var messageDto = new ChatMessageDto
        {
            Id = message.Id,
            SenderId = message.SenderId,
            SenderType = message.SenderType,
            MessageType = message.MessageType,
            Content = message.Content,
            IsRead = message.IsRead,
            CreatedAt = message.CreatedAt,
            Attachments = attachments.Select(a => new ChatAttachmentDto
            {
                Id = a.Id,
                FileType = a.FileType,
                FileUrl = a.FileUrl,
                FileName = a.FileName,
                FileSize = a.FileSize,
                Duration = a.Duration
            }).ToList()
        };

        // ✅ SignalR: إرسال للطرف الآخر
        var recipientId = userType == "customer" ? thread.SupplierId : thread.CustomerId;
        await _chatHubService.SendNewMessageAsync(recipientId, threadId, messageDto);

        // ✅ SignalR: إرسال لكل المتصلين بالمحادثة (تأكيد الاستلام)
        await _chatHubService.SendMessageReceivedAsync(threadId, message.Id, userId, userType);

        // ✅ SignalR: لو أول رسالة في المحادثة، نرسل إشعار NewThread
        var isFirstMessage = await _context.ChatMessages.CountAsync(m => m.ThreadId == threadId && !m.IsDeleted) == 1;
        if (isFirstMessage)
        {
            var customer = await _context.Users.FindAsync(thread.CustomerId);
            var part = thread.PartId.HasValue ? await _context.Parts.FindAsync(thread.PartId) : null;

            await _chatHubService.SendNewThreadAsync(recipientId, new
            {
                ThreadId = threadId,
                CustomerId = thread.CustomerId,
                CustomerName = customer?.FullName,
                CustomerAvatar = customer?.AvatarUrl,
                PartId = thread.PartId,
                PartName = part?.NameAr,
                Message = messageDto,
                CreatedAt = thread.CreatedAt
            });
        }

        return new SendMessageResponse
        {
            Success = true,
            Message = "Message sent",
            MessageAr = "تم إرسال الرسالة",
            ChatMessage = messageDto
        };
    }

    /// <summary>
    /// تعليم المحادثة كمقروءة
    /// </summary>
    public async Task<ChatResponse> MarkAsReadAsync(long userId, string userType, long threadId)
    {
        var thread = await _context.ChatThreads.FindAsync(threadId);

        if (thread == null)
        {
            return new ChatResponse
            {
                Success = false,
                Message = "Thread not found",
                MessageAr = "المحادثة غير موجودة"
            };
        }

        // التحقق من صلاحية الوصول
        if ((userType == "customer" && thread.CustomerId != userId) ||
            (userType == "supplier" && thread.SupplierId != userId))
        {
            return new ChatResponse
            {
                Success = false,
                Message = "Access denied",
                MessageAr = "غير مصرح لك بالوصول لهذه المحادثة"
            };
        }

        // تحديث الرسائل غير المقروءة
        var unreadMessages = await _context.ChatMessages
            .Where(m => m.ThreadId == threadId && !m.IsRead && m.SenderType != userType)
            .ToListAsync();

        if (!unreadMessages.Any())
        {
            return new ChatResponse
            {
                Success = true,
                Message = "No unread messages",
                MessageAr = "لا توجد رسائل غير مقروءة"
            };
        }

        foreach (var msg in unreadMessages)
        {
            msg.IsRead = true;
            msg.ReadAt = DateTime.UtcNow;
        }

        // إعادة تعيين عداد الرسائل غير المقروءة
        if (userType == "customer")
            thread.CustomerUnreadCount = 0;
        else
            thread.SupplierUnreadCount = 0;

        await _context.SaveChangesAsync();

        // ✅ SignalR: إشعار المرسل إن الرسائل اتقرأت
        var senderId = userType == "customer" ? thread.SupplierId : thread.CustomerId;
        var messageIds = unreadMessages.Select(m => m.Id).ToList();

        await _chatHubService.SendMessagesReadAsync(senderId, threadId, userId, userType, messageIds);

        // ✅ SignalR: إرسال لكل المتصلين بالمحادثة
        await _chatHubService.SendMessagesReadToThreadAsync(threadId, userId, userType, messageIds);

        return new ChatResponse
        {
            Success = true,
            Message = "Marked as read",
            MessageAr = "تم التعليم كمقروء"
        };
    }

    /// <summary>
    /// إغلاق/حذف المحادثة (Soft Delete)
    /// </summary>
    public async Task<ChatResponse> CloseThreadAsync(long userId, string userType, long threadId)
    {
        var thread = await _context.ChatThreads.FindAsync(threadId);

        if (thread == null)
        {
            return new ChatResponse
            {
                Success = false,
                Message = "Thread not found",
                MessageAr = "المحادثة غير موجودة"
            };
        }

        // التحقق من صلاحية الوصول
        if ((userType == "customer" && thread.CustomerId != userId) ||
            (userType == "supplier" && thread.SupplierId != userId))
        {
            return new ChatResponse
            {
                Success = false,
                Message = "Access denied",
                MessageAr = "غير مصرح لك بالوصول لهذه المحادثة"
            };
        }

        // Soft Delete
        thread.Status = "closed";
        thread.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // ✅ SignalR: إشعار الطرف الآخر بإغلاق المحادثة
        var otherUserId = userType == "customer" ? thread.SupplierId : thread.CustomerId;
        await _chatHubService.SendThreadClosedAsync(otherUserId, threadId, userId, userType);

        return new ChatResponse
        {
            Success = true,
            Message = "Thread closed successfully",
            MessageAr = "تم إغلاق المحادثة بنجاح"
        };
    }

    #region Helper Methods

    /// <summary>
    /// التحقق من حجم الملفات
    /// </summary>
    private SendMessageResponse ValidateFiles(SendMessageRequest request)
    {
        // التحقق من الصور
        if (request.Images != null)
        {
            foreach (var file in request.Images)
            {
                if (file.Length > MaxImageSize)
                {
                    return new SendMessageResponse
                    {
                        Success = false,
                        Message = $"Image too large. Max size: {MaxImageSize / (1024 * 1024)}MB",
                        MessageAr = $"حجم الصورة كبير جداً. الحد الأقصى: {MaxImageSize / (1024 * 1024)} ميجابايت"
                    };
                }
            }
        }

        // التحقق من الفيديوهات
        if (request.Videos != null)
        {
            foreach (var file in request.Videos)
            {
                if (file.Length > MaxVideoSize)
                {
                    return new SendMessageResponse
                    {
                        Success = false,
                        Message = $"Video too large. Max size: {MaxVideoSize / (1024 * 1024)}MB",
                        MessageAr = $"حجم الفيديو كبير جداً. الحد الأقصى: {MaxVideoSize / (1024 * 1024)} ميجابايت"
                    };
                }
            }
        }

        // التحقق من الصوت
        if (request.Voice != null && request.Voice.Length > MaxVoiceSize)
        {
            return new SendMessageResponse
            {
                Success = false,
                Message = $"Voice too large. Max size: {MaxVoiceSize / (1024 * 1024)}MB",
                MessageAr = $"حجم الملف الصوتي كبير جداً. الحد الأقصى: {MaxVoiceSize / (1024 * 1024)} ميجابايت"
            };
        }

        return new SendMessageResponse { Success = true };
    }

    /// <summary>
    /// تحديد نوع الرسالة
    /// </summary>
    private string DetermineMessageType(bool hasContent, bool hasImages, bool hasVideos, bool hasVoice)
    {
        if (hasVoice) return "voice";
        if (hasImages && hasVideos) return "mixed";
        if (hasImages) return hasContent ? "mixed" : "image";
        if (hasVideos) return hasContent ? "mixed" : "video";
        return "text";
    }

    /// <summary>
    /// إنشاء مرفق
    /// </summary>
    private ChatAttachment CreateAttachment(long messageId, string fileType, string fileUrl, IFormFile file)
    {
        return new ChatAttachment
        {
            MessageId = messageId,
            FileType = fileType,
            FileUrl = fileUrl,
            FileName = file.FileName,
            FileSize = (int)file.Length,
            MimeType = file.ContentType,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// الحصول على نص آخر رسالة للعرض
    /// </summary>
    private string GetLastMessageText(SendMessageRequest request, bool hasContent, bool hasImages, bool hasVideos, bool hasVoice)
    {
        if (hasContent && !string.IsNullOrWhiteSpace(request.Content))
        {
            return request.Content.Length > 100 ? request.Content.Substring(0, 100) + "..." : request.Content;
        }

        if (hasVoice) return "🎤 رسالة صوتية";

        var parts = new List<string>();
        if (hasImages)
        {
            var count = request.Images!.Count;
            parts.Add(count > 1 ? $"📷 {count} صور" : "📷 صورة");
        }
        if (hasVideos)
        {
            var count = request.Videos!.Count;
            parts.Add(count > 1 ? $"🎥 {count} فيديو" : "🎥 فيديو");
        }

        return string.Join(" + ", parts);
    }

    private async Task<ChatThreadDto> MapToThreadDto(ChatThread thread, Part? part, SupplierProfile supplier, long customerId)
    {
        var customer = await _context.Users.FindAsync(customerId);

        return new ChatThreadDto
        {
            Id = thread.Id,
            CustomerId = thread.CustomerId,
            CustomerName = customer?.FullName,
            SupplierId = thread.SupplierId,
            SupplierName = supplier.BusinessNameAr,
            SupplierLogoUrl = supplier.LogoUrl,
            Status = thread.Status,
            CreatedAt = thread.CreatedAt,
            Part = part != null ? new ChatPartDto
            {
                Id = part.Id,
                NameAr = part.NameAr,
                NameEn = part.NameEn,
                Price = part.Price,
                ImageUrl = part.PartImages.FirstOrDefault(i => i.IsPrimary)?.ImageUrl,
                Status = part.Status
            } : null
        };
    }

    #endregion
}