namespace AuthApiBackend.Exceptions.ExceptionTypes
{
    public class InvalidCredentialsException : Exception
    {

        public InvalidCredentialsException(string message) : base(message) { }
    }
}
