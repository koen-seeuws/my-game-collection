using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MyGameCollection.Infrastructure.Identity;

/// <summary>
/// Accounts, roles and invites (ASP.NET Core Identity), plus the data protection keys that sign the login cookies,
/// so sign-ins survive restarts. Lives in the same database as the game collections, in its own tables.
/// </summary>
public sealed class AccountsDbContext : IdentityDbContext<AppUser>, IDataProtectionKeyContext
{
    public AccountsDbContext(DbContextOptions<AccountsDbContext> options) : base(options)
    {
    }

    public DbSet<Invite> Invites => Set<Invite>();
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>(user =>
        {
            user.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
            user.Property(u => u.LastName).HasMaxLength(100).IsRequired();
        });

        builder.Entity<Invite>(invite =>
        {
            invite.ToTable("Invites");
            invite.HasKey(i => i.Id);
            invite.Property(i => i.TokenHash).HasMaxLength(64).IsRequired();
            invite.HasIndex(i => i.TokenHash).IsUnique();
            invite.Property(i => i.Note).HasMaxLength(200);
            invite.Property(i => i.CreatedBy).HasMaxLength(256).IsRequired();
            invite.Property(i => i.RedeemedBy).HasMaxLength(256);
        });
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Everything is stored in UTC; SQLite forgets the kind, so restore it when reading.
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<UtcDateTimeConverter>();
    }

    private sealed class UtcDateTimeConverter()
        : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
            v => v.ToUniversalTime(), v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
}
