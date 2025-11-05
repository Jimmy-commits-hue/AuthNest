using AuthApiBackend.DTOs;

namespace AuthApiBackend.Interfaces.IOperations
{
    public interface ILoginOperation
    {

        Task Login(LoginDto login, CancellationToken cancellationToken);
    }
}
