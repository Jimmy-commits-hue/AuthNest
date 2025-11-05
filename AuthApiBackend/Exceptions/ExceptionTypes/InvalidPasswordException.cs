namespace AuthApiBackend.Exceptions.ExceptionTypes
{

    public class InvalidPasswordException : Exception
    {
        public InvalidPasswordException(string message) : base(message) { }
    }

}
