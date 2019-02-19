using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace PriceTracker.Models
{
    public partial class PriceTrackerContext : DbContext
    {
        public PriceTrackerContext()
        {
        }

        public PriceTrackerContext(DbContextOptions<PriceTrackerContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public virtual DbSet<Result> Result { get; set; }
        public virtual DbSet<SavedSearch> SavedSearch { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySQL(Database.GetDbConnection().ConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Result>(entity =>
            {
                entity.ToTable("Result", "PriceTracker");

                entity.HasIndex(e => e.SavedSearchId)
                    .HasName("SavedSearchID");

                entity.Property(e => e.ResultId)
                    .HasColumnName("ResultID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.AmazonHeading)
                    .HasMaxLength(1000)
                    .IsUnicode(false)
                    .HasDefaultValueSql("NULL");

                entity.Property(e => e.AmazonLink)
                    .IsUnicode(false)
                    .HasDefaultValueSql("NULL");

                entity.Property(e => e.AmazonPrice).HasDefaultValueSql("NULL");

                entity.Property(e => e.Date).HasDefaultValueSql("NULL");

                entity.Property(e => e.EbayHeading)
                    .HasMaxLength(1000)
                    .IsUnicode(false)
                    .HasDefaultValueSql("NULL");

                entity.Property(e => e.EbayLink)
                    .IsUnicode(false)
                    .HasDefaultValueSql("NULL");

                entity.Property(e => e.EbayPrice).HasDefaultValueSql("NULL");

                entity.Property(e => e.JohnLewisHeading)
                    .HasMaxLength(1000)
                    .IsUnicode(false)
                    .HasDefaultValueSql("NULL");

                entity.Property(e => e.JohnLewisLink)
                    .IsUnicode(false)
                    .HasDefaultValueSql("NULL");

                entity.Property(e => e.JohnLewisPrice).HasDefaultValueSql("NULL");

                entity.Property(e => e.SavedSearchId)
                    .HasColumnName("SavedSearchID")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("NULL");

                entity.HasOne(d => d.SavedSearch)
                    .WithMany(p => p.Results)
                    .HasForeignKey(d => d.SavedSearchId)
                    .HasConstraintName("SavedSearchID");
            });

            modelBuilder.Entity<SavedSearch>(entity =>
            {
                entity.ToTable("SavedSearch", "PriceTracker");

                entity.Property(e => e.SavedSearchId)
                    .HasColumnName("SavedSearchID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CreatedDate).HasDefaultValueSql("NULL");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(150)
                    .IsUnicode(false);
            });
        }
    }
}
