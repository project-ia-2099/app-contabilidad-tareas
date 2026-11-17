using Microsoft.EntityFrameworkCore;
using Tareas.Domain;

namespace Tareas.Infrastructure;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public const string Schema = "contabilidad_tareas";

    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.Entity<TaskItem>().Property(item => item.Title).HasMaxLength(TaskItem.TitleLimit);
        modelBuilder.Entity<TaskItem>().HasIndex(item => new { item.BoardId, item.CreatedAt });
    }
}
