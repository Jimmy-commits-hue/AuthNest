namespace AuthApiBackend.Exceptions.ExceptionTypes
{
    public class UserNotFoundException : IOException
    {

        public UserNotFoundException(string Message) : base(Message) { }
 
    }
}
