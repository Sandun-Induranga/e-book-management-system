using EBookPvtLtd.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EBookPvtLtd.Models;

namespace EBookPvtLtd.Data;

public class EBookPvtLtdContext : IdentityDbContext<Users>
{
    public EBookPvtLtdContext(DbContextOptions<EBookPvtLtdContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
        builder.ApplyConfiguration(new UserEntiryConfiguration());
    }

public DbSet<EBookPvtLtd.Models.Customer> Customer { get; set; } = default!;

public DbSet<EBookPvtLtd.Models.Book> Book { get; set; } = default!;

public DbSet<EBookPvtLtd.Models.Order> Order { get; set; } = default!;

public DbSet<EBookPvtLtd.Models.OrderItem> OrderItem { get; set; } = default!;

public DbSet<EBookPvtLtd.Models.Feedback> Feedback { get; set; } = default!;
}

public class UserEntiryConfiguration : IEntityTypeConfiguration<Users>
{
    public void Configure(EntityTypeBuilder<Users> builder)
    {
        builder.Property(u => u.Role).IsRequired();
    }
}
