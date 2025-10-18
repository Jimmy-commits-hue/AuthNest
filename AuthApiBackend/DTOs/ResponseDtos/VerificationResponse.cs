namespace AuthApiBackend.DTOs.ResponseDtos
{
    public class VerificationResponse
    {
        public string UserId { get; set; } = null!;
        public bool IsExpired { get; set; }

        public string Code { get; set; } = null!;
    }
}
