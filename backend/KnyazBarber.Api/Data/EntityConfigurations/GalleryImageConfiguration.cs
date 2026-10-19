using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using KnyazBarber.Api.Entities;

namespace KnyazBarber.Api.Data.EntityConfigurations;

public class GalleryImageConfiguration : IEntityTypeConfiguration<GalleryImage>
{
    public void Configure(EntityTypeBuilder<GalleryImage> b) => b.ComplexProperty(x => x.Caption);
}
