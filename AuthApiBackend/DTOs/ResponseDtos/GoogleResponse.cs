namespace AuthApiBackend.DTOs.ResponseDtos
{
    public class GoogleResponse
    {
        public string? GivenName { get; set; }

        public string? Surname { get; set; }

        public string Email { get; set; } = null!;
    }
    
}
