using Microsoft.AspNetCore.Http;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Tashlih.Application.Interfaces;
using Tashlih.Core.Entities;
using Tashlih.Infrastructure.Models;


namespace Tashlih.Infrastructure.Services
{
    public class LogService : ILogService
    {
        private readonly TashlihContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LogService(TashlihContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(
            long? userId,
            string? userType,
            string? userName,
            string action,
            string actionAr,
            string entityType,
            string entityTypeAr,
            long? entityId,
            object? oldValues = null,
            object? newValues = null,
            string? description = null)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            var log = new Log
            {
                UserId = userId,
                UserType = userType,
                UserName = userName,
                Action = action,
                ActionAr = actionAr,
                EntityType = entityType,
                EntityTypeAr = entityTypeAr,
                EntityId = entityId,
                Description = description,
                OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
                NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
                IpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString(),
                UserAgent = httpContext?.Request?.Headers["User-Agent"].ToString(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Logs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
