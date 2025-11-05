namespace AuthApiBackend.DTOs.ResponseDtos
{
    public record TempPassword
    (
        int AttemptCount,
        string TempPassId
    );
}
