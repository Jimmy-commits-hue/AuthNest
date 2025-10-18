using System.ComponentModel.DataAnnotations;

namespace AuthApiBackend.Models
{

    public class RefreshTokens
    {

        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string UserId { get; set; } = null!;

        public User User { get; set; } = null!;

        public string Token { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }

        public bool IsActive
        {
            get
            {
                return ExpiresAt > DateTime.UtcNow;
            }
        }

    }

}
