using System.ComponentModel.DataAnnotations;

namespace AuthApiBackend.Models
{

    public class TemporaryPassword
    {

        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string AccountId { get; set; } = null!;

        public Account Account { get; set; } = null!;

        public string HashedPassword { get; set; } = null!;

        public int AttemptCount { get; set; }

        public bool IsEmailSent { get; set; }

        public long ExpiresAt { get; set; }

        public bool IsActive { get; set; }

        public bool IsExpired
        {
            get
            {
                return DateTimeOffset.UtcNow.ToUnixTimeSeconds() > ExpiresAt;
            }
        }
    }

}
