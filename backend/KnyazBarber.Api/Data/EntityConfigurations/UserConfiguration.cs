using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using KnyazBarber.Api.Entities;

namespace KnyazBarber.Api.Data.EntityConfigurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.Property(x => x.Login).HasMaxLength(64);
        b.HasIndex(x => x.Login).IsUnique();
        b.Property(x => x.Email).HasMaxLength(256);
        b.Property(x => x.TelegramUsername).HasMaxLength(32);
        b.HasIndex(x => x.TelegramUsername).IsUnique();
        b.HasIndex(x => x.TelegramUserId).IsUnique();
        b.HasOne(x => x.Barber).WithMany().HasForeignKey(x => x.BarberId).OnDelete(DeleteBehavior.SetNull);
    }
}
