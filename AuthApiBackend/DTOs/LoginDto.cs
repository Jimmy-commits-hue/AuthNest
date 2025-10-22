using System.ComponentModel.DataAnnotations;

namespace AuthApiBackend.DTOs
{
    public class LoginDto
    {

        [Required(ErrorMessage ="Please fill in this field")]
        public string LoginNumber { get; set; } = null!;

        [Required(ErrorMessage ="Please fill in your password")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*[@!&?#])(?=.*[1-9]).{8}$", ErrorMessage = "Invalid Password")]
        public string Password { get; set; } = null!;

    }

}
