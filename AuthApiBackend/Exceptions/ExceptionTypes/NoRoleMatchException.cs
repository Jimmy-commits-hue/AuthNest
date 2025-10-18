namespace AuthApiBackend.Exceptions.ExceptionTypes
{

    public class NoRoleMatchException : IOException
    {

        public NoRoleMatchException(string Message) : base(Message) { }

    }

}
