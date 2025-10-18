namespace AuthApiBackend.Exceptions.ExceptionTypes
{
    public class CodeExpiredException : IOException
    {
        public CodeExpiredException(string message) : base(message) { }
    }
}
