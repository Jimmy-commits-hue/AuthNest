namespace AuthApiBackend.Exceptions.ExceptionTypes
{
    public class NoAccountMatchException : IOException
    {
        public NoAccountMatchException(string message) : base(message) { }
    }
}
