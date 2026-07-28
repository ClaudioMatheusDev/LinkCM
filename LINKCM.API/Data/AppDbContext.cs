using LinkCM.Models;
using Microsoft.EntityFrameworkCore;

namespace LinkCM.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<URLCurta> ShortUrls => Set<URLCurta>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<URLCurta>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.UrlOriginal)
                    .IsRequired()
                    .HasMaxLength(2048);

                entity.Property(x => x.ShortCode)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.HasIndex(x => x.ShortCode)
                    .IsUnique();
            });

        }
}
}