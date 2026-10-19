using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using KnyazBarber.Api.Entities;

namespace KnyazBarber.Api.Data.EntityConfigurations;

public class BarberConfiguration : IEntityTypeConfiguration<Barber>
{
    public void Configure(EntityTypeBuilder<Barber> b)
    {
        b.Property(x => x.Name).HasMaxLength(100);
        b.ComplexProperty(x => x.Title);
        b.ComplexProperty(x => x.Bio);
        b.HasMany(x => x.WorkingHours).WithOne().HasForeignKey(x => x.BarberId);
        b.HasMany(x => x.TimeOffs).WithOne().HasForeignKey(x => x.BarberId);
    }
}
