namespace AuthApiBackend.Exceptions.ExceptionTypes
{
    public class AccountLockedException : Exception
    {

        public AccountLockedException(string Message) : base(Message) { }
    }
}
