using AuthApiBackend.DTOs;

namespace AuthApiBackend.Interfaces.IOperations
{
    public interface IResetPasswordRequest
    {
        Task RequestResetPassword(PasswordReset resetPassword, CancellationToken cancellationToken);
    }
}
