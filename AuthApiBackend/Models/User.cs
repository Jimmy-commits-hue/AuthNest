using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AuthApiBackend.Models
{

    public class User
    {

        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string IdNumber { get; set; } = string.Empty;

        public string FirstName { get; set; } = null!;

        public string Surname { get; set; } = null!;

        public DateOnly RegistrationDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

        public UserRole UserRole { get; set; } = null!;

        public ContactDetails? ContactDetails { get; set; }

        public Account? Account { get; set; }

        public ICollection<RefreshTokens> RefreshTokens { get; set; } = new HashSet<RefreshTokens>();

    }

}
