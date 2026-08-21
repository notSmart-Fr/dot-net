namespace TaskApi.Common;

using Microsoft.EntityFrameworkCore;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TaskEntity> Tasks => Set<TaskEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TaskEntity>(entity =>
        {
            entity.ToTable("tasks");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired();
            entity.Property(e => e.Done).HasDefaultValue(false);

            // Stage 0 Initial Seed Data
            entity.HasData(
                new TaskEntity { Id = 1, Title = "Buy milk", Done = false },
                new TaskEntity { Id = 2, Title = "Learn C# Minimal APIs", Done = false },
                new TaskEntity { Id = 3, Title = "Complete assignment", Done = true }
            );
        });
    }
}

public class TaskEntity
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    
    public bool Done { get; set; }
}