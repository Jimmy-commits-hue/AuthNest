namespace AuthApiBackend.DTOs.ResponseDtos
{
    public record ResetPasswordResponse(string email, string password, string tempPasswordId);
}
