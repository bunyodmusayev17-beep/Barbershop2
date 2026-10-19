using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using KnyazBarber.Api.Entities;

namespace KnyazBarber.Api.Data.EntityConfigurations;

public class WorkingHoursConfiguration : IEntityTypeConfiguration<WorkingHours>
{
    public void Configure(EntityTypeBuilder<WorkingHours> b)
    {
        b.HasIndex(x => new {x.BarberId, x.DayOfWeek}).IsUnique();
    }
}
