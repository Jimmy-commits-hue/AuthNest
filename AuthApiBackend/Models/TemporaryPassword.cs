using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using System.Text.Json;

namespace AuthApiBackend.Models
{

    public class TemporaryPassword
    {

        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string AccountId { get; set; } = null!;

        public Account Account { get; set; } = null!;

        public string HashedPassword { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        public bool IsEmailSent { get; set; }

        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    }

}
