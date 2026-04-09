using Microsoft.EntityFrameworkCore;
using PhoneNumberApi.Models;

namespace PhoneNumberApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {

    }
    public DbSet<PhoneNumber> PhoneNumbers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PhoneNumber>(entity =>
        {
            entity.ToTable("phone_numbers");

            entity.HasIndex(e => e.Number)
                  .IsUnique();

            entity.Property(e => e.Number)
                  .IsRequired()
                  .HasMaxLength(20);
        });
    }
}