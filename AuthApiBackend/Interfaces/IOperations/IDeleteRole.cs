namespace AuthApiBackend.Interfaces.IOperations
{
    public interface IDeleteRole
    {

        Task Delete(string roleName, CancellationToken cancellationToken);
    }
}
