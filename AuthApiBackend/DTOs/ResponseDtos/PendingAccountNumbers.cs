namespace AuthApiBackend.DTOs.ResponseDtos
{
    public record PendingAccountNumbers
    (
        string AccountNumber,
        string Email, 
        string AccountId 
    );
}
