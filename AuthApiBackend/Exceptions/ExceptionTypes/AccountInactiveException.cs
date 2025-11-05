namespace AuthApiBackend.Exceptions.ExceptionTypes
{
    public class AccountInactiveException : Exception
    {

        public AccountInactiveException(string message) : base(message) { }
    }
}
