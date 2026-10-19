namespace KnyazBarber.Api.Dtos.Review;

public record ReviewAdminDto(int Id, string AuthorName, int Rating, string Text, bool IsPublished, DateTimeOffset CreatedAt);
