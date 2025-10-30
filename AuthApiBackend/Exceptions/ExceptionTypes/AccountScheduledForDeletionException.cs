namespace AuthApiBackend.Exceptions.ExceptionTypes
{
    public class AccountScheduledForDeletionException : Exception
    {

        public AccountScheduledForDeletionException(string Message) : base(Message) { }
    }
}
