namespace AuthApiBackend.Exceptions.ExceptionTypes
{
    public class UserAlreadyExistException : Exception
    {
        
        public UserAlreadyExistException(string Message) : base(Message) { }

    }
}
