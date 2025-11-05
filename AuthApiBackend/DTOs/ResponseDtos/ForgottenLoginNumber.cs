namespace AuthApiBackend.DTOs.ResponseDtos
{

    public record ForgottenLoginNumber
    (
        string LoginNumber,
        string UserEmail
    );

}
