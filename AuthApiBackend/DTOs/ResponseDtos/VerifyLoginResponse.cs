namespace AuthApiBackend.DTOs.ResponseDtos
{
    public record VerifyLoginResponse
    (
        int LoginAttempt,
        bool IsLocked,
        Enums.AccountStatus Status
    );
    
}
