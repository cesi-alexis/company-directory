using Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.context
{
    // Contexte de base de données pour Entity Framework
    public class BusinessDirectoryDbContext : DbContext
    {
        public DbSet<Site> Sites { get; set; } = null!;
        public DbSet<Service> Services { get; set; } = null!;
        public DbSet<Employee> Employees { get; set; } = null!;

        public BusinessDirectoryDbContext(DbContextOptions<BusinessDirectoryDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration des contraintes et relations supplémentaires
            modelBuilder.Entity<Site>().Property(s => s.City).IsRequired().HasMaxLength(50);
            modelBuilder.Entity<Site>()
                .HasIndex(e => e.City)
                .IsUnique();
            modelBuilder.Entity<Service>().Property(s => s.Name).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<Service>()
                .HasIndex(e => e.Name)
                .IsUnique();
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Site)
                .WithMany(s => s.Employees)
                .HasForeignKey(e => e.SiteId)
                .OnDelete(DeleteBehavior.Restrict); // Empêche la suppression si des employés sont liés
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Service)
                .WithMany(s => s.Employees)
                .HasForeignKey(e => e.ServiceId)
                .OnDelete(DeleteBehavior.Restrict); // Empêche la suppression si des employés sont liés
            modelBuilder.Entity<Employee>().Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            modelBuilder.Entity<Employee>().Property(e => e.LastName).IsRequired().HasMaxLength(50);
            modelBuilder.Entity<Employee>().Property(e => e.Email).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<Employee>()
                .HasIndex(e => e.Email)
                .IsUnique();
            modelBuilder.Entity<Employee>().Property(e => e.PhoneFixed).HasMaxLength(15);
            modelBuilder.Entity<Employee>().Property(e => e.PhoneMobile).HasMaxLength(15);
        }
    }
}