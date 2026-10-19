using KnyazBarber.Api.Dtos.Review;

namespace KnyazBarber.Api.Services;

public interface IReviewService
{
    Task<List<ReviewAdminDto>> ListAsync(CancellationToken ct);
    Task<ReviewAdminDto> CreateAsync(ReviewInput input, CancellationToken ct);
    Task UpdateAsync(int id, ReviewInput input, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
}
