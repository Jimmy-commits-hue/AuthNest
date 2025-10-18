namespace AuthApiBackend.Exceptions.ExceptionTypes
{

    public class RoleAlreadyExistException : IOException
    {

        public RoleAlreadyExistException(string Message) : base(Message) { }

    }

}
