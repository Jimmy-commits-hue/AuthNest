using AuthApiBackend.DTOs;

namespace AuthApiBackend.Interfaces.IOperations
{
    public interface ICancelDeletion
    {
        Task CancelAccountDeletion(LoginDto login, CancellationToken cancellationToken);
    }
}
