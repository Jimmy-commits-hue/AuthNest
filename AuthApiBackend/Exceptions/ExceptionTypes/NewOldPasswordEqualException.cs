namespace AuthApiBackend.Exceptions.ExceptionTypes
{
    public class NewOldPasswordEqualException : Exception
    {

        public NewOldPasswordEqualException(string message): base(message) { }
    }
}
