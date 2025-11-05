namespace AuthApiBackend.DTOs.ResponseDtos
{

    public record UserResponse
    (
        string UserId,
        int AttemptCount
    );

}
