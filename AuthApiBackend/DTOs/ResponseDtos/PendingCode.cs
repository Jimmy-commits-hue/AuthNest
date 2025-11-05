namespace AuthApiBackend.DTOs.ResponseDtos
{

    public record PendingCode
    (
        string Id,
        string Code,
        string Email,
        string FirstName,
        string Surname
    );

}
