using System.ComponentModel.DataAnnotations;

namespace AuthApiBackend.Models
{

    public class ContactDetails
    {

        [Key]
        public string UserId { get; set; } = string.Empty;

        public User User { get; set; } = null!;

        public string Email { get; set; } = null!;

        public bool IsEmailVerified { get; set; } = false;

        public ICollection<User> users { get; set; } = new HashSet<User>();

        public ICollection<VerificationCode> VerificationCode { get; set; } = new HashSet<VerificationCode>();


    }

}
