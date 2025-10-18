using System.ComponentModel.DataAnnotations;

namespace AuthApiBackend.Models
{

    public class VerificationCode
    {

        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string EmailId { get; set; } = null!;

        public ContactDetails ContactDetails { get; set; } = null!;

        public string Code { get; set; } = null!;

        public long ExpiresAt { get; set; }

        public bool IsExpired
        {
            get
            {
                return DateTimeOffset.UtcNow.ToUnixTimeSeconds() > ExpiresAt;
            }
        }

        public bool IsEmailSent { get; set; } = false;

        public int AttemptCount { get; set; }

        public bool IsActive { get; set; }

    }

}
