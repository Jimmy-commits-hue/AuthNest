namespace AuthApiBackend.DTOs.ResponseDtos
{
    public record VerificationResponse
    (
        string UserId,
        bool IsExpired,
        string Code
    );

}
