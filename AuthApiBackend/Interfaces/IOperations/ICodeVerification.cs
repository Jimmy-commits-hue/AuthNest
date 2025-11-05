using AuthApiBackend.DTOs;

namespace AuthApiBackend.Interfaces.IOperations
{
    public interface ICodeVerification
    {

        Task VerifyCode(CodeVerificationDto code, CancellationToken cancellationToken);

    }
}
