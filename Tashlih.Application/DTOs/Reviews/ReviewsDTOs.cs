namespace Tashlih.Application.DTOs.Reviews;

#region Request DTOs

/// <summary>
/// طلب إضافة تقييم
/// </summary>
public class CreateReviewRequest
{
    public long OrderId { get; set; }
    public int Rating { get; set; }  // 1-5
    public string? Comment { get; set; }
}

/// <summary>
/// طلب تعديل تقييم
/// </summary>
public class UpdateReviewRequest
{
    public int Rating { get; set; }  // 1-5
    public string? Comment { get; set; }
}

#endregion

#region Response DTOs

/// <summary>
/// استجابة التقييم
/// </summary>
public class ReviewResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? MessageAr { get; set; }
    public ReviewDto? Review { get; set; }
}

/// <summary>
/// استجابة قائمة التقييمات
/// </summary>
public class ReviewsListResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? MessageAr { get; set; }
    public List<ReviewDto>? Reviews { get; set; }
    public decimal RatingAverage { get; set; }
    public int RatingCount { get; set; }
    public RatingBreakdownDto? RatingBreakdown { get; set; }
}

/// <summary>
/// استجابة بسيطة
/// </summary>
public class ReviewBaseResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? MessageAr { get; set; }
}

#endregion

#region Data DTOs

/// <summary>
/// بيانات التقييم
/// </summary>
public class ReviewDto
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public long CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerAvatar { get; set; }
    public long SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// توزيع التقييمات
/// </summary>
public class RatingBreakdownDto
{
    public int Five { get; set; }   // عدد تقييمات 5 نجوم
    public int Four { get; set; }   // عدد تقييمات 4 نجوم
    public int Three { get; set; }  // عدد تقييمات 3 نجوم
    public int Two { get; set; }    // عدد تقييمات 2 نجوم
    public int One { get; set; }    // عدد تقييمات 1 نجمة
}

/// <summary>
/// ملخص التقييمات للمورد
/// </summary>
public class SupplierReviewsSummaryDto
{
    public decimal RatingAverage { get; set; }
    public int RatingCount { get; set; }
    public RatingBreakdownDto? RatingBreakdown { get; set; }
    public List<ReviewDto>? RecentReviews { get; set; }
}

#endregion