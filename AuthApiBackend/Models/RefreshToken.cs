using System.ComponentModel.DataAnnotations;

namespace AuthApiBackend.Models
{

    public class RefreshToken
    {

        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string AccountId { get; set; } = null!;

        public Account Account { get; set; } = null!;

        public string Token { get; set; } = null!;

        public long ExpiresAt { get; set; }

        public bool IsActive
        {
            get
            {
                return ExpiresAt > DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
        }

    }

}
