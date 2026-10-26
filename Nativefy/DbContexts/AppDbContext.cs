using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Nativefy.Engine;
using Nativefy.Models.Media;

namespace Nativefy.DbContexts;

public sealed partial class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AppStateEntity> AppStates { get; set; }
    public DbSet<SearchResultEntity> SearchCache { get; set; }
    public DbSet<OnlinePlaylist> SavedOnlinePlaylists { get; set; }
    public DbSet<Playlist> SavedOfflinePlaylists { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppStateEntity>().OwnsMany(e => e.LikedItems, builder => builder.ToJson());

        modelBuilder.Entity<OnlinePlaylist>(entity => entity.ToTable("SavedOnlinePlaylists"));

        modelBuilder.Entity<Playlist>(entity =>
        {
            entity.ToTable("SavedOfflinePlaylists");
            entity.OwnsMany(p => p.Items, builder => builder.ToJson());
        });

        modelBuilder.Entity<SearchResultEntity>(entity =>
        {
            entity.ToTable("SearchCache");
            entity.OwnsMany(s => s.Results, builder => builder.ToJson());
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