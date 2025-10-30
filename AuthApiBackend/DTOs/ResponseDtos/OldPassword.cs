namespace AuthApiBackend.DTOs.ResponseDtos
{

    public record OldPassword(string OldUserPassword, Enums.AccountStatus Status, string accountId, bool IsLocked);

}
