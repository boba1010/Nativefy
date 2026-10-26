using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nativefy.DbContexts;

public sealed partial class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Playlist> Playlists { get; set; }

    public DbSet<Track> Tracks { get; set; }
    public DbSet<PlaylistTrack> PlaylistTracks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Composite key setup for playlist-to-track junction table
        modelBuilder.Entity<PlaylistTrack>()
            .HasKey(pt => new { pt.PlaylistId, pt.VideoId });

        // Explicitly set up the many-to-many relationship mappings
        modelBuilder.Entity<PlaylistTrack>()
            .HasOne(pt => pt.Playlist)
            .WithMany(p => p.Tracks)
            .HasForeignKey(pt => pt.PlaylistId);

        modelBuilder.Entity<PlaylistTrack>()
            .HasOne(pt => pt.Track)
            .WithMany(t => t.PlaylistTracks)
            .HasForeignKey(pt => pt.VideoId);
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