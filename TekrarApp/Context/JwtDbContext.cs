using Microsoft.EntityFrameworkCore;
using TekrarApp.Model;

namespace TekrarApp.Context
{
    public class JwtDbContext : DbContext
    {
        public JwtDbContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(user => user.UserRoles);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(role => role.UserRoles);

            modelBuilder.Entity<User>().Property(x => x.IsConfirmEmail).HasDefaultValue(false);
        }
    }
}
