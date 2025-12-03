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

            public DbSet<Role> Role { get; set; }

            public DbSet<UserRole> UserRole { get; set; }

            public DbSet<TemporaryPassword> TemporaryPassword { get; set; }

            public DbSet<RefreshToken> RefreshToken { get; set; }

            public DbSet<BlackListedToken> BlackListedToken { get; set; }

           private readonly DatabaseSettings _settings;

            public AuthApiDbContext(IOptions<DatabaseSettings> options)
            {
                _settings = options.Value;
            }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {

                int retryCount = 5;
                string connectionString = string.Empty;
                while (retryCount > 0)
                {

                    try
                    {
                        string Password = Environment.GetEnvironmentVariable("DB_PASS")!;
                        string password = string.Empty;

                        if (string.IsNullOrEmpty(Password) || Password.Contains('\\'))
                        {
                            Password = Environment.GetEnvironmentVariable("DB_PASSWORD")!;
                            password = Password.Split('\\')[1];
                        }
                        else
                        {
                            password = Password;
                        }

                        string Server = Environment.GetEnvironmentVariable("DB_SERVER") ?? _settings.Server;
                        string Port = Environment.GetEnvironmentVariable("DB_PORT") ?? _settings.Port;
                        string Database = Environment.GetEnvironmentVariable("DB_DATABASE") ?? _settings.Database;
                        string User = Environment.GetEnvironmentVariable("DB_USER") ?? _settings.User;
                        Console.WriteLine("Password + +++: " + Password + " " + Server + " " + Port + " " +
                            Database + " " + User);
                        connectionString = $"Server={Server};Port={Port};Database={Database};User={User};Password={password};";
                        break;
                    }
                    catch
                    {
                        throw new Exception("Database connection failed");
                    }
                }




                optionsBuilder.UseMySql(
                    connectionString,
                    MySqlServerVersion.AutoDetect(connectionString),
                    mySqlOptions => mySqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 10,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null
                    )
                );

                Console.WriteLine("Successfully connected to the database.");
                base.OnConfiguring(optionsBuilder);
                Console.WriteLine("Successfully connected to the database 2");
            } 
            }
            
          // public AuthApiDbContext(DbContextOptions<AuthApiDbContext> options) : base(options) { }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<BlackListedToken>().HasKey(u => u.Id);

                modelBuilder.Entity<User>().HasIndex(u => u.Id).IsUnique();

                modelBuilder.Entity<User>().HasOne(u => u.ContactDetails).WithOne(u => u.User).HasForeignKey<ContactDetails>(u => u.Id).
                                            OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<User>().HasOne(u => u.Account).WithOne(u => u.User).HasForeignKey<Account>(u => u.Id).
                                            OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<ContactDetails>().HasMany(u => u.VerificationCode).WithOne(u => u.ContactDetails).HasForeignKey(u => u.EmailId).
                                            OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<Account>().HasMany(u => u.RefreshTokens).WithOne(u => u.Account).HasForeignKey(u => u.AccountId).
                                           OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<Role>().HasMany(u => u.UserRole).WithOne(u => u.Role).HasForeignKey(u => u.RoleId).
                                           OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<User>().HasOne(u => u.UserRole).WithOne(u => u.User).HasForeignKey<UserRole>(u => u.Id).
                                           OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<Account>().HasMany(u => u.TemporaryPassword).WithOne(u => u.Account).HasForeignKey(u => u.AccountId).
                                               OnDelete(DeleteBehavior.Cascade);

                base.OnModelCreating(modelBuilder);

            }

    }

}