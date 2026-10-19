using KnyazBarber.Api.Entities;

namespace KnyazBarber.Api.Dtos.Gallery;

public record GalleryUpdate(LocalizedText? Caption, int SortOrder);
