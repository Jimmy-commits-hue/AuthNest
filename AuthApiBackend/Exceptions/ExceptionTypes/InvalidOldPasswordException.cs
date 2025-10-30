namespace AuthApiBackend.Exceptions.ExceptionTypes
{
    public class InvalidOldPasswordException : Exception
    {

        public InvalidOldPasswordException(string Message) : base(Message) { }
    }
}
