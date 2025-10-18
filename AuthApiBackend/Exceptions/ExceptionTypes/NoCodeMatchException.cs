namespace AuthApiBackend.Exceptions.ExceptionTypes
{
    public class NoCodeMatchException : IOException
    {
        public NoCodeMatchException(string message) : base(message) { }
    }
}
