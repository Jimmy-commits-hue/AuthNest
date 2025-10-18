namespace AuthApiBackend.DTOs.ResponseDtos
{
    public class PendingCode
    {
        public string Id { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string Surname { get; set; } = null!;
    }
}
