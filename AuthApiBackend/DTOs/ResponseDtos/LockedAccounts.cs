namespace AuthApiBackend.DTOs.ResponseDtos
{
    public record LockedAccounts
    (
        string accountId,
        string Email
    );
}
