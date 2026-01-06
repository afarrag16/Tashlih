using Microsoft.EntityFrameworkCore;
using Tashlih.Application.DTOs.Admin;
using Tashlih.Infrastructure.Models;

namespace Tashlih.Infrastructure.Services;

public class AdminLogsService
{
    private readonly TashlihContext _context;

    public AdminLogsService(TashlihContext context)
    {
        _context = context;
    }

    /// <summary>
    /// عرض كل الـ Logs
    /// </summary>
    public async Task<LogsResponse> GetLogsAsync(LogsRequest request)
    {
        var query = _context.Logs.AsQueryable();

        // فلترة بنوع العملية
        if (!string.IsNullOrEmpty(request.Action))
        {
            query = query.Where(l => l.Action == request.Action);
        }

        // فلترة بنوع الكيان
        if (!string.IsNullOrEmpty(request.EntityType))
        {
            query = query.Where(l => l.EntityType == request.EntityType);
        }

        // فلترة بالمستخدم
        if (request.UserId.HasValue)
        {
            query = query.Where(l => l.UserId == request.UserId);
        }

        // فلترة بنوع المستخدم
        if (!string.IsNullOrEmpty(request.UserType))
        {
            query = query.Where(l => l.UserType == request.UserType);
        }

        // فلترة بالتاريخ من
        if (request.FromDate.HasValue)
        {
            query = query.Where(l => l.CreatedAt >= request.FromDate.Value);
        }

        // فلترة بالتاريخ إلى
        if (request.ToDate.HasValue)
        {
            query = query.Where(l => l.CreatedAt <= request.ToDate.Value);
        }

        // البحث
        if (!string.IsNullOrEmpty(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(l =>
                (l.Description != null && l.Description.ToLower().Contains(search)) ||
                (l.UserName != null && l.UserName.ToLower().Contains(search)));
        }

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);

        var logs = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(l => new LogDto
            {
                Id = l.Id,
                UserId = l.UserId,
                UserType = l.UserType,
                UserName = l.UserName,
                Action = l.Action,
                ActionAr = l.ActionAr,
                EntityType = l.EntityType,
                EntityTypeAr = l.EntityTypeAr,
                EntityId = l.EntityId,
                Description = l.Description,
                OldValues = l.OldValues,
                NewValues = l.NewValues,
                IpAddress = l.IpAddress,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        return new LogsResponse
        {
            Success = true,
            Logs = logs,
            TotalCount = totalItems,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = totalPages
        };
    }

    /// <summary>
    /// تفاصيل Log
    /// </summary>
    public async Task<LogDetailResponse> GetLogByIdAsync(long logId)
    {
        var log = await _context.Logs.FindAsync(logId);

        if (log == null)
        {
            return new LogDetailResponse
            {
                Success = false,
                Message = "Log not found",
                MessageAr = "السجل غير موجود"
            };
        }

        return new LogDetailResponse
        {
            Success = true,
            Log = new LogDto
            {
                Id = log.Id,
                UserId = log.UserId,
                UserType = log.UserType,
                UserName = log.UserName,
                Action = log.Action,
                ActionAr = log.ActionAr,
                EntityType = log.EntityType,
                EntityTypeAr = log.EntityTypeAr,
                EntityId = log.EntityId,
                Description = log.Description,
                OldValues = log.OldValues,
                NewValues = log.NewValues,
                IpAddress = log.IpAddress,
                UserAgent = log.UserAgent,
                CreatedAt = log.CreatedAt
            }
        };
    }
}
