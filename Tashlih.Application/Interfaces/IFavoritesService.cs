using Tashlih.Application.DTOs.Favorites;

namespace Tashlih.Application.Interfaces;

public interface IFavoritesService
{
    // القطع المفضلة
    Task<FavoriteResponse> AddPartToFavoritesAsync(long customerId, long partId);
    Task<FavoriteResponse> RemovePartFromFavoritesAsync(long customerId, long partId);
    Task<FavoritePartsResponse> GetFavoritePartsAsync(long customerId);
    Task<FavoriteCheckResponse> IsPartFavoriteAsync(long customerId, long partId);

    // الموردين المفضلين
    Task<FavoriteResponse> AddSupplierToFavoritesAsync(long customerId, long supplierId);
    Task<FavoriteResponse> RemoveSupplierFromFavoritesAsync(long customerId, long supplierId);
    Task<FavoriteSuppliersResponse> GetFavoriteSuppliersAsync(long customerId);
    Task<FavoriteCheckResponse> IsSupplierFavoriteAsync(long customerId, long supplierId);
}
