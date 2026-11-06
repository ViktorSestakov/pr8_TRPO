using Microsoft.EntityFrameworkCore;

namespace pr8_TRPO.Models;

// Этот контекст вместе с моделями пересоздаётся через dotnet ef dbcontext scaffold.
public partial class Pr8Context : DbContext
{
    public Pr8Context(DbContextOptions<Pr8Context> options) : base(options) { }

    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Task> Tasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Users");
            entity.HasIndex(e => e.Login).IsUnique();
            entity.Property(e => e.Login).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Role).HasMaxLength(20).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(512).IsRequired();
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Tasks");
            entity.HasIndex(e => e.OwnerLogin);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.OwnerLogin).HasMaxLength(100).IsRequired();
            entity.Property(e => e.IsCompleted).HasDefaultValue(false);
        });
    }
}
