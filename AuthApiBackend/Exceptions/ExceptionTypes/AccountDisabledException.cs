namespace AuthApiBackend.Exceptions.ExceptionTypes
{
    public class AccountDisabledException : Exception
    {

        public AccountDisabledException(string Message) : base(Message) { }
    }
}
