using System.ComponentModel.DataAnnotations;

namespace AuthApiBackend.DTOs
{
    public class RoleDto
    {

        [Required]
        [MaxLength(10)]
        public string RoleName { get; set; } = null!;

    }
}
