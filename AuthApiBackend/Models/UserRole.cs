using System.ComponentModel.DataAnnotations;

namespace AuthApiBackend.Models
{

    public class UserRole
    {

        [Key]
        public string UserId { get; set; } = null!;

        public User User { get; set; } = null!;

        public int? RoleId { get; set; }

        public Role Role { get; set; } = null!;

    }

}
