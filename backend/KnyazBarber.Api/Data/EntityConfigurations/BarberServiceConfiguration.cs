using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using KnyazBarber.Api.Entities;

namespace KnyazBarber.Api.Data.EntityConfigurations;

public class BarberServiceConfiguration : IEntityTypeConfiguration<BarberService>
{
    public void Configure(EntityTypeBuilder<BarberService> b)
    {
        b.HasKey(x => new {x.BarberId, x.ServiceId});
        b.HasOne(x => x.Barber).WithMany(x => x.Services).HasForeignKey(x => x.BarberId);
        b.HasOne(x => x.Service).WithMany(x => x.Barbers).HasForeignKey(x => x.ServiceId);
    }
}
