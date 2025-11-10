using AuthApiBackend.Enums;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace AuthApiBackend.Models
{

    public class Account
    {
        [Key]
        public string Id { get; set; } = string.Empty;

        public User User { get; set; } = null!;

        public string AccountNumber { get; set; } = string.Empty;

        public string Password { get; set; } = null!;

        public bool IsActive
        {
            get
            {
                return !string.IsNullOrEmpty(AccountNumber);
            }
        }

        public AccountStatus Status { get; set; } = AccountStatus.Pending;

        public bool IsEmailSent { get; set; } = false;

        public int FailedLoginAttempts { get; set; }

        public bool IsLocked
        {
            get
            {
                return LockOutUntilDate.HasValue && LockOutUntilDate > DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
        }

        public long? LockOutUntilDate { get; set; }

        public long ExpectedDeleteDate { get; set; } 

        public ICollection<TemporaryPassword> TemporaryPassword { get; set; } = [];

        public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    }

}
