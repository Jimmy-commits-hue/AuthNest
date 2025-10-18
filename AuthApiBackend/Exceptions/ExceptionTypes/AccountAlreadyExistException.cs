namespace AuthApiBackend.Exceptions.ExceptionTypes
{
    public class AccountAlreadyExistException : IOException
    {
        public AccountAlreadyExistException(string message) : base(message) { }
    }
}
