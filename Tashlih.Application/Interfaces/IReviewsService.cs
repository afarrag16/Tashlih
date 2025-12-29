using Tashlih.Application.DTOs.Reviews;

namespace Tashlih.Application.Interfaces;

public interface IReviewsService
{
    // للعميل
    Task<ReviewResponse> CreateReviewAsync(long customerId, CreateReviewRequest request);
    Task<ReviewResponse> UpdateReviewAsync(long customerId, long reviewId, UpdateReviewRequest request);
    Task<ReviewBaseResponse> DeleteReviewAsync(long customerId, long reviewId);
    Task<ReviewsListResponse> GetMyReviewsAsync(long customerId);

    // للكل
    Task<ReviewsListResponse> GetSupplierReviewsAsync(long supplierId, int page = 1, int pageSize = 10);
    Task<SupplierReviewsSummaryDto> GetSupplierReviewsSummaryAsync(long supplierId);
}
