using AuthApiBackend.DTOs;

namespace AuthApiBackend.Interfaces.IOperations
{
    public interface IRegistration
    {

        Task Register(RegisterDto user, CancellationToken cancellationToken);
    }
}
