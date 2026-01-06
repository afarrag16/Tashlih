using System.Threading.Tasks;

namespace Tashlih.Application.Interfaces
{
    public interface ILogService
    {
        Task LogAsync(
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
            string? description = null
        );
    }
}
