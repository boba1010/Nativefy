using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Nativefy.Models.Media;

namespace Nativefy.DbContexts;

public sealed partial class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public const string SingletonLikedMediaId = "LIKED_MEDIA_SINGLETON";

    public DbSet<LikedMediaPlaylist> LikedMedia { get; set; }
    public DbSet<LocalPlaylist> SavedPlaylists { get; set; }

    public DbSet<Engine.SearchResultEntity> SearchCache { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LikedMediaPlaylist>(entity =>
        {
            entity.ToTable("LikedMedia", t => t.HasCheckConstraint(
                "CK_LikedMedia_SingleRow",
                $"[Id] = '{SingletonLikedMediaId}'"
            ));
            entity.Property(p => p.Id).ValueGeneratedNever();
            entity.OwnsMany(p => p.Items, builder => builder.ToJson());
        });

        modelBuilder.Entity<LocalPlaylist>(entity =>
        {
            entity.ToTable("SavedPlaylists");
            entity.OwnsMany(p => p.Items, builder => builder.ToJson());
        });

        modelBuilder.Entity<Engine.SearchResultEntity>(entity =>
        {
            entity.ToTable("SearchCache");
            //entity.OwnsMany(s => s.Results, builder => builder.ToJson());
        });
    }
}

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // This configuration is only used to generate the migration files
        optionsBuilder.UseSqlite("Data Source=design_time_temp.db");

        return new AppDbContext(optionsBuilder.Options);
    }
}