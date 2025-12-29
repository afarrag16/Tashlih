using Microsoft.EntityFrameworkCore;
using Tashlih.Application.DTOs.Reviews;
using Tashlih.Application.DTOs.Notification;
using Tashlih.Application.Interfaces;
using Tashlih.Core.Entities;
using Tashlih.Infrastructure.Models;

namespace Tashlih.Infrastructure.Services;

public class ReviewsService : IReviewsService
{
    private readonly TashlihContext _context;
    private readonly INotificationService _notificationService;

    public ReviewsService(TashlihContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    #region Customer Methods

    /// <summary>
    /// إضافة تقييم جديد
    /// </summary>
    public async Task<ReviewResponse> CreateReviewAsync(long customerId, CreateReviewRequest request)
    {
        // التحقق من الطلب
        var order = await _context.Orders
            .Include(o => o.Supplier)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.CustomerId == customerId);

        if (order == null)
        {
            return new ReviewResponse
            {
                Success = false,
                Message = "Order not found",
                MessageAr = "الطلب غير موجود"
            };
        }

        // التحقق من حالة الطلب (يجب أن يكون مستلم)
        if (order.Status != "received")
        {
            return new ReviewResponse
            {
                Success = false,
                Message = "You can only review after receiving the order",
                MessageAr = "يمكنك التقييم فقط بعد استلام الطلب"
            };
        }

        // التحقق من عدم وجود تقييم سابق
        var existingReview = await _context.Reviews
            .FirstOrDefaultAsync(r => r.OrderId == request.OrderId);

        if (existingReview != null)
        {
            return new ReviewResponse
            {
                Success = false,
                Message = "You have already reviewed this order",
                MessageAr = "لقد قمت بتقييم هذا الطلب مسبقاً"
            };
        }

        // التحقق من التقييم (1-5)
        if (request.Rating < 1 || request.Rating > 5)
        {
            return new ReviewResponse
            {
                Success = false,
                Message = "Rating must be between 1 and 5",
                MessageAr = "التقييم يجب أن يكون بين 1 و 5"
            };
        }

        // إنشاء التقييم
        var review = new Review
        {
            OrderId = request.OrderId,
            CustomerId = customerId,
            SupplierId = order.SupplierId,
            OverallRating = (byte)request.Rating,
            Comment = request.Comment,
            IsVerified = true,
            IsVisible = true,
            IsReported = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        // تحديث تقييم المورد
        await UpdateSupplierRatingAsync(order.SupplierId);

        // إرسال إشعار للمورد
        var customer = await _context.Users.FindAsync(customerId);
        await _notificationService.SendReviewNotificationAsync(
            review.Id,
            order.SupplierId,
            customer?.FullName ?? "عميل",
            request.Rating
        );

        return new ReviewResponse
        {
            Success = true,
            Message = "Review added successfully",
            MessageAr = "تم إضافة التقييم بنجاح",
            Review = await MapToDto(review)
        };
    }

    /// <summary>
    /// تعديل تقييم
    /// </summary>
    public async Task<ReviewResponse> UpdateReviewAsync(long customerId, long reviewId, UpdateReviewRequest request)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == reviewId && r.CustomerId == customerId);

        if (review == null)
        {
            return new ReviewResponse
            {
                Success = false,
                Message = "Review not found",
                MessageAr = "التقييم غير موجود"
            };
        }

        // التحقق من التقييم (1-5)
        if (request.Rating < 1 || request.Rating > 5)
        {
            return new ReviewResponse
            {
                Success = false,
                Message = "Rating must be between 1 and 5",
                MessageAr = "التقييم يجب أن يكون بين 1 و 5"
            };
        }

        // تحديث التقييم
        review.OverallRating = (byte)request.Rating;
        review.Comment = request.Comment;
        review.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // تحديث تقييم المورد
        await UpdateSupplierRatingAsync(review.SupplierId);

        return new ReviewResponse
        {
            Success = true,
            Message = "Review updated successfully",
            MessageAr = "تم تعديل التقييم بنجاح",
            Review = await MapToDto(review)
        };
    }

    /// <summary>
    /// حذف تقييم
    /// </summary>
    public async Task<ReviewBaseResponse> DeleteReviewAsync(long customerId, long reviewId)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == reviewId && r.CustomerId == customerId);

        if (review == null)
        {
            return new ReviewBaseResponse
            {
                Success = false,
                Message = "Review not found",
                MessageAr = "التقييم غير موجود"
            };
        }

        var supplierId = review.SupplierId;

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        // تحديث تقييم المورد
        await UpdateSupplierRatingAsync(supplierId);

        return new ReviewBaseResponse
        {
            Success = true,
            Message = "Review deleted successfully",
            MessageAr = "تم حذف التقييم بنجاح"
        };
    }

    /// <summary>
    /// جلب تقييماتي
    /// </summary>
    public async Task<ReviewsListResponse> GetMyReviewsAsync(long customerId)
    {
        var reviews = await _context.Reviews
            .Where(r => r.CustomerId == customerId && r.IsVisible)
            .Include(r => r.Supplier)
            .Include(r => r.Order)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var reviewsDto = new List<ReviewDto>();
        foreach (var review in reviews)
        {
            reviewsDto.Add(await MapToDto(review));
        }

        return new ReviewsListResponse
        {
            Success = true,
            Reviews = reviewsDto,
            RatingCount = reviews.Count
        };
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// جلب تقييمات مورد
    /// </summary>
    public async Task<ReviewsListResponse> GetSupplierReviewsAsync(long supplierId, int page = 1, int pageSize = 10)
    {
        var query = _context.Reviews
            .Where(r => r.SupplierId == supplierId && r.IsVisible)
            .Include(r => r.Customer)
            .Include(r => r.Order)
            .OrderByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync();

        var reviews = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var reviewsDto = new List<ReviewDto>();
        foreach (var review in reviews)
        {
            reviewsDto.Add(await MapToDto(review));
        }

        // حساب الإحصائيات
        var stats = await GetRatingStatsAsync(supplierId);

        return new ReviewsListResponse
        {
            Success = true,
            Reviews = reviewsDto,
            RatingAverage = stats.Average,
            RatingCount = stats.Count,
            RatingBreakdown = stats.Breakdown
        };
    }

    /// <summary>
    /// جلب ملخص تقييمات المورد (للملف الشخصي)
    /// </summary>
    public async Task<SupplierReviewsSummaryDto> GetSupplierReviewsSummaryAsync(long supplierId)
    {
        var stats = await GetRatingStatsAsync(supplierId);

        var recentReviews = await _context.Reviews
            .Where(r => r.SupplierId == supplierId && r.IsVisible)
            .Include(r => r.Customer)
            .Include(r => r.Order)
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .ToListAsync();

        var reviewsDto = new List<ReviewDto>();
        foreach (var review in recentReviews)
        {
            reviewsDto.Add(await MapToDto(review));
        }

        return new SupplierReviewsSummaryDto
        {
            RatingAverage = stats.Average,
            RatingCount = stats.Count,
            RatingBreakdown = stats.Breakdown,
            RecentReviews = reviewsDto
        };
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// تحديث تقييم المورد
    /// </summary>
    private async Task UpdateSupplierRatingAsync(long supplierId)
    {
        var stats = await _context.Reviews
            .Where(r => r.SupplierId == supplierId && r.IsVisible)
            .GroupBy(r => r.SupplierId)
            .Select(g => new
            {
                Count = g.Count(),
                Average = g.Average(r => (decimal)r.OverallRating)
            })
            .FirstOrDefaultAsync();

        var supplier = await _context.SupplierProfiles.FindAsync(supplierId);
        if (supplier != null)
        {
            supplier.RatingCount = stats?.Count ?? 0;
            supplier.RatingAverage = Math.Round(stats?.Average ?? 0, 1);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// جلب إحصائيات التقييم
    /// </summary>
    private async Task<(decimal Average, int Count, RatingBreakdownDto Breakdown)> GetRatingStatsAsync(long supplierId)
    {
        var reviews = await _context.Reviews
            .Where(r => r.SupplierId == supplierId && r.IsVisible)
            .ToListAsync();

        var count = reviews.Count;
        var average = count > 0 ? Math.Round((decimal)reviews.Average(r => r.OverallRating), 1) : 0;

        var breakdown = new RatingBreakdownDto
        {
            Five = reviews.Count(r => r.OverallRating == 5),
            Four = reviews.Count(r => r.OverallRating == 4),
            Three = reviews.Count(r => r.OverallRating == 3),
            Two = reviews.Count(r => r.OverallRating == 2),
            One = reviews.Count(r => r.OverallRating == 1)
        };

        return (average, count, breakdown);
    }

    /// <summary>
    /// تحويل Entity لـ DTO
    /// </summary>
    private async Task<ReviewDto> MapToDto(Review review)
    {
        var customer = review.Customer ?? await _context.Users.FindAsync(review.CustomerId);
        var order = review.Order ?? await _context.Orders.FindAsync(review.OrderId);
        var supplier = review.Supplier ?? await _context.SupplierProfiles.FindAsync(review.SupplierId);

        return new ReviewDto
        {
            Id = review.Id,
            OrderId = review.OrderId,
            OrderNumber = order?.OrderNumber,
            CustomerId = review.CustomerId,
            CustomerName = customer?.FullName,
            CustomerAvatar = customer?.AvatarUrl,
            SupplierId = review.SupplierId,
            SupplierName = supplier?.BusinessNameAr,
            Rating = review.OverallRating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt
        };
    }

    #endregion
}
