namespace KnyazBarber.Api.Dtos.Public;

public record PublicReview(int Id, string AuthorName, int Rating, string Text, DateTimeOffset CreatedAt);
