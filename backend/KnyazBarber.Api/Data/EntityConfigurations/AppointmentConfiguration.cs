using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using KnyazBarber.Api.Entities;

namespace KnyazBarber.Api.Data.EntityConfigurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> b)
    {
        b.Property(x => x.ClientName).HasMaxLength(100);
        b.Property(x => x.ClientPhone).HasMaxLength(20);
        b.Property(x => x.Comment).HasMaxLength(500);
        b.Property(x => x.Language).HasMaxLength(2);
        b.Property(x => x.Price).HasPrecision(12, 0);
        b.HasIndex(x => new {x.BarberId, x.StartUtc});
        b.HasIndex(x => x.ClientPhone);
        b.HasOne(x => x.Service).WithMany().HasForeignKey(x => x.ServiceId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Barber).WithMany().HasForeignKey(x => x.BarberId).OnDelete(DeleteBehavior.Restrict);
        b.ToTable(t => t.HasCheckConstraint("CK_Appointment_Range", "\"EndUtc\" > \"StartUtc\""));
        // Ikki navbat bir ustada vaqt bo'yicha kesishmasligi — migratsiyada EXCLUDE cheklovi qo'shiladi.
    }
}
