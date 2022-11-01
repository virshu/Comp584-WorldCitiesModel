using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace WorldCitiesModel.Models
{
    public partial class WorldCitiesContext : IdentityDbContext<WorldCitiesUser>
    {
        public WorldCitiesContext()
        {
        }

        public WorldCitiesContext(DbContextOptions<WorldCitiesContext> options)
            : base(options)
        {
        }

        public virtual DbSet<City> Cities { get; set; }
        public virtual DbSet<Country> Countries { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationBuilder builder = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false)
                    .AddJsonFile("appsettings.development.json", optional: true);
                IConfigurationRoot configuration = builder.Build();
                optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<City>(entity =>
            {
                entity.HasOne(d => d.Country)
                    .WithMany(p => p.Cities)
                    .HasForeignKey(d => d.CountryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Cities_Countries");
            });

            modelBuilder.Entity<Country>(entity =>
            {
                entity.Property(e => e.Iso2).IsFixedLength();

                entity.Property(e => e.Iso3).IsFixedLength();
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
