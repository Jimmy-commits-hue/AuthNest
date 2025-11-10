namespace AuthApiBackend.Interfaces.IOperations
{
    public interface ILogoutOperation
    {
        Task Logout(CancellationToken cancellationToken);
    }
}
