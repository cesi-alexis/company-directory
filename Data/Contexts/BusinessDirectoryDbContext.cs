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
            // Configuration des contraintes et relations supplémentaires
            modelBuilder.Entity<Site>().Property(s => s.City).IsRequired().HasMaxLength(50);
            modelBuilder.Entity<Service>().Property(s => s.Name).IsRequired().HasMaxLength(100);

            modelBuilder.Entity<Employee>().Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            modelBuilder.Entity<Employee>().Property(e => e.LastName).IsRequired().HasMaxLength(50);
            modelBuilder.Entity<Employee>().Property(e => e.Email).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<Employee>().Property(e => e.PhoneFixed).HasMaxLength(15);
            modelBuilder.Entity<Employee>().Property(e => e.PhoneMobile).HasMaxLength(15);
        }
    }
}