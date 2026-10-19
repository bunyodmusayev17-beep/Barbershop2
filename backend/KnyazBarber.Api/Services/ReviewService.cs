using Microsoft.EntityFrameworkCore;
using KnyazBarber.Api.Dtos.Review;
using KnyazBarber.Api.Entities;
using KnyazBarber.Api.Exceptions;
using KnyazBarber.Api.Repositories;

namespace KnyazBarber.Api.Services;

public class ReviewService(IBaseRepository<Review> reviews) : IReviewService
{
    public Task<List<ReviewAdminDto>> ListAsync(CancellationToken ct) =>
        reviews.QueryNoTracking().OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewAdminDto(r.Id, r.AuthorName, r.Rating, r.Text, r.IsPublished, r.CreatedAt)).ToListAsync(ct);

    public async Task<ReviewAdminDto> CreateAsync(ReviewInput input, CancellationToken ct)
    {
        var r = await reviews.AddAsync(new Review
        {
            AuthorName = input.AuthorName.Trim(),
            Rating = input.Rating,
            Text = input.Text.Trim(),
            IsPublished = input.IsPublished,
        }, ct);
        return new ReviewAdminDto(r.Id, r.AuthorName, r.Rating, r.Text, r.IsPublished, r.CreatedAt);
    }

    public async Task UpdateAsync(int id, ReviewInput input, CancellationToken ct)
    {
        var r = await reviews.GetByIdAsync(id, ct) ?? throw new NotFoundException("Sharh topilmadi");
        r.AuthorName = input.AuthorName.Trim();
        r.Rating = input.Rating;
        r.Text = input.Text.Trim();
        r.IsPublished = input.IsPublished;
        await reviews.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var r = await reviews.GetByIdAsync(id, ct) ?? throw new NotFoundException("Sharh topilmadi");
        await reviews.DeleteAsync(r, ct);
    }
}
