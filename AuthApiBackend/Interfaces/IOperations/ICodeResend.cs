namespace AuthApiBackend.Interfaces.IOperations
{
    public interface ICodeResend
    {

        Task ResendCode(string idNumber, CancellationToken cancellationToken);
    }
}
