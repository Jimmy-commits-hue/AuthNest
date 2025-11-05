using AuthApiBackend.DTOs;

namespace AuthApiBackend.Interfaces.IOperations
{
    public interface IResetPassword
    {
        Task PasswordReset(ResetPasswordDto verifyPassword, CancellationToken cancellationToken);
    }
}
