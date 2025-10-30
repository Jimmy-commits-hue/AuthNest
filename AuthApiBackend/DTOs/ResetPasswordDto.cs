using System.ComponentModel.DataAnnotations;

namespace AuthApiBackend.DTOs
{

    public class ResetPasswordDto
    {

        [Required]
        [DataType(DataType.Password)]
        public string TemporaryPassword { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Password mismatch")]
        public string ConfirmPassword { get; set;} = null!;

        [Required]
        public string TempPasswordId { get; set; } = null!;

    }

}
