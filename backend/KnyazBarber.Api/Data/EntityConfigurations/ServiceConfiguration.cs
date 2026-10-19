using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using KnyazBarber.Api.Entities;

namespace KnyazBarber.Api.Data.EntityConfigurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> b)
    {
        b.ComplexProperty(x => x.Name);
        b.ComplexProperty(x => x.Description);
        b.Property(x => x.Price).HasPrecision(12, 0);
    }
}
