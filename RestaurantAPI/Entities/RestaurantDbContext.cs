﻿using Microsoft.EntityFrameworkCore;

namespace RestaurantAPI.Entities
{
    public class RestaurantDbContext : DbContext
    {
        // ConnectionString został zdefiniowany w appsettings.Development.json
        //private string _connectionString = "Server=localhost\\SQLEXPRESS;Database=Restaurant;Trusted_Connection=True;";

        public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options) : base(options)
        {
            
        }

        public DbSet<Restaurant>? Restaurants { get; set; }
        public DbSet<Address>? Addresses { get; set; }
        public DbSet<Dish>? Dishes { get; set; }
        public DbSet<User>? Users { get; set; }
        public DbSet<Role>? Roles { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer(_connectionString);
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Restaurant>()
                .Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(25);

            modelBuilder.Entity<Dish>()
                .Property(d => d.Name)
                .IsRequired();

            modelBuilder.Entity<Address>()
                .Property(a => a.City)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Address>()
                .Property(a => a.Street)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.DateOfBirth)
                .IsRequired();

            modelBuilder.Entity<Role>()
                .Property(r => r.Name)
                .IsRequired();

            // Poniżej metoda do dodania seedów (rekordów) do bazy danych
            //modelBuilder.Entity<Restaurant>()
            //    .HasData(new
            //    {

            //    });
        }
    }
}
