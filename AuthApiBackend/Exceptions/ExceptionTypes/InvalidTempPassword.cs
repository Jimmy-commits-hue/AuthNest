namespace AuthApiBackend.Exceptions.ExceptionTypes
{
    public class InvalidTempPassword : Exception
    {

        public InvalidTempPassword(string Message) : base(Message) { }
    }
}
