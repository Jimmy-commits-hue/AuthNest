using System.ComponentModel.DataAnnotations.Schema;

namespace AuthApiBackend.Models
{
    public class Role
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string RoleName { get; set; } = null!;

        public ICollection<UserRole> UserRole = new List<UserRole>();

    }

}
