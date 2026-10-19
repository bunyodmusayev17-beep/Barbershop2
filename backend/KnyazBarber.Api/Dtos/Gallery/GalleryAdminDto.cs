using KnyazBarber.Api.Entities;

namespace KnyazBarber.Api.Dtos.Gallery;

public record GalleryAdminDto(int Id, string Url, LocalizedText Caption, int SortOrder);
