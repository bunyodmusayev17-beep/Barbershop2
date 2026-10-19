using System.ComponentModel.DataAnnotations;

namespace KnyazBarber.Api.Dtos.Review;

public record ReviewInput([Required, MaxLength(100)] string AuthorName, [Range(1, 5)] int Rating, [Required, MaxLength(1000)] string Text, bool IsPublished);
