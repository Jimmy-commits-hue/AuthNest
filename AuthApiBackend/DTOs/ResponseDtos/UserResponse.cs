namespace AuthApiBackend.DTOs.ResponseDtos
{
    public class UserResponse
    {
        public string UserId { get; set; } = null!;

        public int AttemptCount { get; set; }
    }
}
