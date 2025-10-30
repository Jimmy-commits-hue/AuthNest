using System.ComponentModel.DataAnnotations;

namespace AuthApiBackend.DTOs
{

    public class UpdatePasswordDto
    {

        [Required]
        [DataType(DataType.Text)]
        public string loginNumber { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Password mismatch")]
        public string ConfirmPassword { get; set; } = null!;

    }

}
