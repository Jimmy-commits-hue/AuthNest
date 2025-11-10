using System.ComponentModel.DataAnnotations;

namespace AuthApiBackend.Models
{

    public class BlackListedToken
    {
        [Key]
        public string? Id { get; set; }

        public string? TokenId { get; set; }

        public long ExpiresIn { get; set; }
    }

}