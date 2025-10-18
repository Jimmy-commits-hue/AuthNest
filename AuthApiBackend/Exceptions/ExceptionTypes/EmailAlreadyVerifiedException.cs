namespace AuthApiBackend.Exceptions.ExceptionTypes
{
    public class EmailAlreadyVerifiedException : IOException
    {
        public EmailAlreadyVerifiedException(string? message) : base(message) { }
    }
}
