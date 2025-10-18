using AuthApiBackend.Configurations;
using AuthApiBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AuthApiBackend.Database
{
    public class AuthApiDbContext : DbContext
    {
            public DbSet<User> User { get; set; }

            public DbSet<ContactDetails> ContactDetails { get; set; }

            public DbSet<Account> Account { get; set; }

            public DbSet<VerificationCode> VerificationCode { get; set; }

            public DbSet<RefreshTokens> RefreshToken { get; set; }

            public DbSet<Role> Role { get; set; }

            public DbSet<UserRole> UserRole { get; set; }


            private readonly DatabaseSettings _settings;

            public AuthApiDbContext(IOptions<DatabaseSettings> options)
            {
                _settings = options.Value;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {

                if (!optionsBuilder.IsConfigured)
                {

                    string Password = Environment.GetEnvironmentVariable("DB_PASSWORD")!;
                    string password = Password.Split('\\')[1];

                    string connectionString = $"Server={_settings.Server};Port={_settings.Port};User={_settings.User};" +
                        $"Database={_settings.Database};Password={password};";

                    optionsBuilder.UseMySql(connectionString, MySqlServerVersion.AutoDetect(connectionString));

                    base.OnConfiguring(optionsBuilder);
                }

            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {

                modelBuilder.Entity<User>().HasIndex(u => u.Id).IsUnique();

                modelBuilder.Entity<User>().HasOne(u => u.ContactDetails).WithOne(u => u.User).HasForeignKey<ContactDetails>(u => u.UserId).
                                            OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<User>().HasOne(u => u.Account).WithOne(u => u.User).HasForeignKey<Account>(u => u.UserId).
                                            OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<ContactDetails>().HasMany(u => u.VerificationCode).WithOne(u => u.ContactDetails).HasForeignKey(u => u.EmailId).
                                            OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<User>().HasMany(u => u.RefreshTokens).WithOne(u => u.User).HasForeignKey(u => u.UserId).
                                           OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<Role>().HasMany(u => u.UserRole).WithOne(u => u.Role).HasForeignKey(u => u.RoleId).
                                           OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<User>().HasOne(u => u.UserRole).WithOne(u => u.User).HasForeignKey<UserRole>(u => u.UserId).
                                           OnDelete(DeleteBehavior.Cascade);

                base.OnModelCreating(modelBuilder);

            }
    }
}
